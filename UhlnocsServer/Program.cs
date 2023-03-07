using UhlnocsServer.Database;
using UhlnocsServer.Services;
using UhlnocsServer.Users;

namespace UhlnocsServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();

            builder.Services.AddTransient<ApplicationDatabaseContext>();

            builder.Services.AddTransient<IRepository<User>, Repository<User>>();

            var app = builder.Build();

            app.MapGrpcService<UserService>();

            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}