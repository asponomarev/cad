using Grpc.Core;
using UhlnocsServer.Database;
using UhlnocsServer.Users;
using static UhlnocsServer.Utils.PropertiesHolder;
using static UhlnocsServer.Utils.ExceptionUtils;

namespace UhlnocsServer.Services
{
    public class UserService : UserServiceProto.UserServiceProtoBase
    {
        private static readonly List<string> Admins = AdminSettings.GetSection("admins").Get<List<string>>();

        private readonly ILogger<UserService> Logger;
        private readonly IRepository<User> UsersRepository;

        public UserService(ILogger<UserService> logger, IRepository<User> usersRepository)
        {
            Logger = logger;
            UsersRepository = usersRepository;
        }

        public static bool IsNotAdmin(string userId)
        {
            return !Admins.Contains(userId);
        }

        public async Task<User> AuthenticateUser(ServerCallContext context)
        {
            string? userId = context.RequestHeaders.GetValue("user");
            if (userId == null)
            {
                string exceptionMessage = "No data for authentication was provided in request";
                throw new RpcException(new Status(StatusCode.Unauthenticated, exceptionMessage));
            }

            User? user = null;
            try
            {
                user = await UsersRepository.GetById(userId);
            } catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            if (user == null)
            {
                string exceptionMessage = $"Unknown user '{userId}'";
                throw new RpcException(new Status(StatusCode.Unauthenticated, exceptionMessage));
            }
            return user;
        }

        public override async Task<UserIdMessage> SignUpUser(UserWithoutIdRequest request, ServerCallContext context)
        {
            User user = new(
                Guid.NewGuid().ToString(),
                request.Email,
                request.Password,
                request.Name,
                request.Surname,
                request.Description
            );

            try
            {
                await UsersRepository.Create(user);
            } catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            return new UserIdMessage
            {
                Id = user.Id
            };
        }

        public override Task<UserFullDataMessage> SignInUser(UserSignInRequest request, ServerCallContext context)
        {
            User? user = null;
            try
            {
                user = UsersRepository.Get().FirstOrDefault(u => u.Email == request.Email);
            } catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }  
            
            if (user == null)
            {
                string exceptionMessage = $"User with email '{request.Email}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }
            if (user.Password != request.Password)
            {
                string exceptionMessage = $"Password '{request.Password}' is incorrect for user with email '{request.Email}'";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            return Task.FromResult(
                new UserFullDataMessage {
                    Id = user.Id,
                    Email = user.Email,
                    Password = user.Password,
                    Name = user.Name,
                    Surname = user.Surname,
                    Description = user.Description
                }
            );
        }

        public override async Task<EmptyMessage> DeleteUser(UserIdMessage request, ServerCallContext context)
        {
            User sender = await AuthenticateUser(context);
            if (IsNotAdmin(sender.Id)) 
            {
                string exceptionMessage = "User deletion is considered an unsafe operation and only admins may perform it";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            try
            {
                await UsersRepository.Delete(request.Id);
            } catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            return new EmptyMessage { };
        }

        public override async Task<EmptyMessage> UpdateUser(UserFullDataMessage request, ServerCallContext context)
        {
            User sender = await AuthenticateUser(context);
            if (sender.Id != request.Id && IsNotAdmin(sender.Id))
            {
                string exceptionMessage = $"User with id '{sender.Id}' is not allowed to update user with id '{request.Id}'";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            sender.Email = request.Email;
            sender.Password = request.Password;
            sender.Name = request.Name;
            sender.Surname = request.Surname;
            sender.Description = request.Description;

            try
            {
                await UsersRepository.Update(sender);
            } catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            return new EmptyMessage { };
        }

        public override async Task<UserFullDataMessage> GetUserFullData(UserIdMessage request,  ServerCallContext context)
        {
            User sender = await AuthenticateUser(context);
            if (sender.Id != request.Id && IsNotAdmin(sender.Id))
            {
                string exceptionMessage = $"User with id '{sender.Id}' is not allowed to access confident data of other users";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            User? result = null;
            try
            {
                result = await UsersRepository.GetById(request.Id);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            if (result == null)
            {
                string exceptionMessage = $"User with id '{request.Id}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            return new UserFullDataMessage
            {
                Id = result.Id,
                Email = result.Email,
                Password = result.Password,
                Name = result.Name,
                Surname = result.Surname,
                Description = result.Description
            };
        }

        public override async Task<UserOpenDataReply> GetUserOpenData(UserIdMessage request, ServerCallContext context)
        {
            await AuthenticateUser(context);

            User? result = null;
            try
            {
                result = await UsersRepository.GetById(request.Id);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            if (result == null)
            {
                string exceptionMessage = $"User with id '{request.Id}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            return new UserOpenDataReply
            {
                Id = result.Id,
                Email = result.Email,
                Name = result.Name,
                Surname = result.Surname,
                Description = result.Description
            };
        }

        public override async Task<UsersOpenDataReply> GetUsersOpenData(UsersIdsRequest request, ServerCallContext context)
        {
            await AuthenticateUser(context);

            List<string> usersIds = new List<string>();
            foreach (UserIdMessage idMessage in request.Ids)
            {
                usersIds.Add(idMessage.Id);
            }

            List<User> users = new List<User>();
            try
            {
                users = UsersRepository.Get().Where(u => usersIds.Contains(u.Id)).ToList();
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            UsersOpenDataReply usersOpenData = new UsersOpenDataReply { };
            foreach (User user in users)
            {
                UserOpenDataReply userOpenData = new UserOpenDataReply
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                    Description = user.Description,
                };
                usersOpenData.Users.Add(userOpenData);
            }
            return usersOpenData;
        }

        public override async Task<UsersOpenDataReply> GetAllUsersOpenData(EmptyMessage request, ServerCallContext context)
        {
            await AuthenticateUser(context);

            List<User> users = new List<User>();
            try
            {
                users = UsersRepository.Get().ToList();
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            UsersOpenDataReply usersOpenData = new UsersOpenDataReply { };
            foreach (User user in users)
            {
                UserOpenDataReply userOpenData = new UserOpenDataReply 
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                    Description = user.Description,
                };
                usersOpenData.Users.Add(userOpenData);
            }
            return usersOpenData;
        }
    }
}
