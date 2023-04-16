using Google.Protobuf;
using Grpc.Core;
using System.IO.Compression;
using System.Text.Json;
using UhlnocsServer.Database;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Users;
using static UhlnocsServer.Utils.ExceptionUtils;
using static UhlnocsServer.Utils.PropertiesHolder;

namespace UhlnocsServer.Services
{
    public class ModelService : ModelServiceProto.ModelServiceProtoBase
    {
        private readonly ILogger<ModelService> Logger;
        private readonly IRepository<Model> ModelsRepository;
        private readonly UserService UserService;

        public readonly string TmpDirectory = ModelSettings.GetValue<string>("tmpDirectory");
        public readonly string ModelsDirectory = ModelSettings.GetValue<string>("modelsDirectory");
        public readonly int BufferSize = ModelSettings.GetValue<int>("modelServiceBufferSize");

        // TODO: add lock on assign to these two variables
        public static volatile Dictionary<string, ParameterWithModels> ParametersWithModels = new();
        public static volatile Dictionary<string, CharacteristicWithModels> CharacteristicsWithModels = new();

        public ModelService(ILogger<ModelService> logger, IRepository<Model> modelsRepository, UserService userService)
        {
            Logger = logger;
            ModelsRepository = modelsRepository;
            UserService = userService;
            SetPropertiesInfoWithModels();
        }

        private void SetPropertiesInfoWithModels()
        {
            List<Model> models = new();
            try
            {
                models = ModelsRepository.Get().ToList();
            }
            catch (Exception exception)
            {
                ThrowInternalException(exception);
            }

            Dictionary<string, ParameterWithModels> newParametersWithModels = new();
            Dictionary<string, CharacteristicWithModels> newCharacteristicsWithModels = new();
            foreach (Model model in models)
            {
                ModelConfiguration configuration = ModelConfiguration.FromJsonDocument(model.Configuration);

                foreach (ParameterInfo parameterInfo in configuration.ParametersInfo)
                {
                    if (newParametersWithModels.ContainsKey(parameterInfo.Id))
                    {
                        newParametersWithModels[parameterInfo.Id].Models.Add(model.Id);
                    }
                    else
                    {
                        ParameterWithModels newParameterWithModels = new(parameterInfo.Id, parameterInfo.Name,
                                                                         parameterInfo.ValueType, new List<string> { model.Id });
                        newParametersWithModels.Add(parameterInfo.Id, newParameterWithModels);
                    }
                }
                
                foreach (CharacteristicInfo characteristicInfo in configuration.CharacteristicsInfo)
                {
                    if (newCharacteristicsWithModels.ContainsKey(characteristicInfo.Id))
                    {
                        newCharacteristicsWithModels[characteristicInfo.Id].Models.Add(model.Id);
                    }
                    else
                    {
                        CharacteristicWithModels newCharacteristicWithModels = new(characteristicInfo.Id, characteristicInfo.Name,
                                                                                   characteristicInfo.ValueType, new List<string> { model.Id });
                        newCharacteristicsWithModels.Add(characteristicInfo.Id, newCharacteristicWithModels);
                    }
                }
            }
            ParametersWithModels = newParametersWithModels;
            CharacteristicsWithModels = newCharacteristicsWithModels;
        }

        public override async Task<ParametersWithModelsReply> GetParametersWithModels(ModelEmptyMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            string parametersWithModelsJson = JsonSerializer.Serialize(ParametersWithModels, PropertyBase.PropertySerializerOptions);
            return new ParametersWithModelsReply
            {
                ParametersWithModelsJson = parametersWithModelsJson
            };
        }

        public override async Task<CharacteristicsWithModelsReply> GetCharacteristicsWithModels(ModelEmptyMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            string characteristicsWithModelsJson = JsonSerializer.Serialize(CharacteristicsWithModels, PropertyBase.PropertySerializerOptions);
            return new CharacteristicsWithModelsReply
            {
                CharacteristicsWithModelsJson = characteristicsWithModelsJson
            };
        }

        public override async Task<ModelEmptyMessage> AddModelConfiguration(ModelConfigurationMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            JsonDocument configurationJsonDocument = null;
            ModelConfiguration configuration = null;
            try
            {
                configurationJsonDocument = JsonDocument.Parse(request.ModelConfigurationJson);
                configuration = ModelConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception) 
            {
                ThrowBadRequestException(exception);
            }

            Model model = new(configuration.Id, configurationJsonDocument);
            try
            {
                await ModelsRepository.Create(model);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }
            SetPropertiesInfoWithModels();

            return new ModelEmptyMessage
            {

            };
        }

        public override async Task<ModelConfigurationMessage> GetModelConfiguration(ModelIdRequest request,  ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(request.ModelId);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            if (model == null)
            {
                string exceptionMessage = $"Model with id '{request.ModelId}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            string modelConfigurationJson = JsonSerializer.Serialize(model.Configuration);
            return new ModelConfigurationMessage
            {
                ModelConfigurationJson = modelConfigurationJson
            };
        }

        public override async Task<ModelEmptyMessage> UpdateModelConfiguration(ModelConfigurationMessage request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            JsonDocument newConfigurationJsonDocument = null;
            ModelConfiguration newConfiguration = null;
            try
            {
                newConfigurationJsonDocument = JsonDocument.Parse(request.ModelConfigurationJson);
                newConfiguration = ModelConfiguration.FromJsonDocument(newConfigurationJsonDocument);
            }
            catch (Exception exception)
            {
                ThrowBadRequestException(exception);
            }

            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(newConfiguration.Id);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }
            if (model == null)
            {
                string exceptionMessage = $"Model with id '{newConfiguration.Id}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            ModelConfiguration oldConfiguration = ModelConfiguration.FromJsonDocument(model.Configuration);
            if (!oldConfiguration.ResponsibleUsers.Contains(sender.Id) && UserService.IsNotAdmin(sender.Id))
            {
                string exceptionMessage = $"User with id '{sender.Id}' is not allowed to update model with id '{oldConfiguration.Id}'";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            model.Configuration = newConfigurationJsonDocument;
            try
            {
                await ModelsRepository.Update(model);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }
            SetPropertiesInfoWithModels();

            return new ModelEmptyMessage
            {

            };
        }

        public override async Task<ModelEmptyMessage> DeleteModel(ModelIdRequest request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);
            if (UserService.IsNotAdmin(sender.Id))
            {
                string exceptionMessage = "Model deletion is considered an unsafe operation and only admins may perform it";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            try
            {
                string modelDirectory = ModelsDirectory + request.ModelId;
                if (Directory.Exists(modelDirectory))
                {
                    Directory.Delete(modelDirectory, true);
                }

                await ModelsRepository.Delete(request.ModelId);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            return new ModelEmptyMessage
            {

            };
        }

        // this method was not tested properly and may contain bugs
        public override async Task<ModelEmptyMessage> UploadModelArchive(IAsyncStreamReader<ArchivePartRequest> requestStream,
                                                                         ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            string modelDirectory = string.Empty;
            string modelZipFilePath = string.Empty;
            byte[] buffer = new byte[BufferSize];
            FileStream fs = null;
            bool first = true;
            await foreach (ArchivePartRequest archivePart in requestStream.ReadAllAsync())
            {
                if (first)
                {
                    first = false;

                    Model? model = null;
                    try
                    {
                        model = await ModelsRepository.GetById(archivePart.ModelId);
                    }
                    catch (Exception exception)
                    {
                        ThrowUnknownException(exception);
                    }
                    if (model == null)
                    {
                        string exceptionMessage = $"Model with id '{archivePart.ModelId}' not found";
                        throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
                    }

                    ModelConfiguration configuration = ModelConfiguration.FromJsonDocument(model.Configuration);
                    if (!configuration.ResponsibleUsers.Contains(sender.Id) && UserService.IsNotAdmin(sender.Id))
                    {
                        string exceptionMessage = $"User with id '{sender.Id}' is not allowed to upload model with id '{configuration.Id}'";
                        throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
                    }

                    modelDirectory = ModelsDirectory + configuration.Id;
                    if (Directory.Exists(modelDirectory))
                    {
                        try
                        {
                            Directory.Delete(modelDirectory, true);
                        }
                        catch (Exception exception)
                        {
                            ThrowUnknownException(exception);
                        }
                    }

                    modelZipFilePath = TmpDirectory + model.Id + "-" + Guid.NewGuid().ToString();
                    fs = File.OpenWrite(modelZipFilePath);
                }

                buffer = archivePart.Data.ToByteArray();
                int bytesToRead = archivePart.BytesToRead;
                try
                {
                    fs.Write(buffer, 0, bytesToRead);
                }
                catch (Exception exception)
                {
                    fs.Close();
                    ThrowUnknownException(exception);
                }
            }

            try
            {
                fs.Close();
                ZipFile.ExtractToDirectory(modelZipFilePath, modelDirectory);
                File.Delete(modelZipFilePath);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }

            return new ModelEmptyMessage
            {

            };
        }

        // this method was not tested properly and may contain bugs
        public override async Task DownloadModelArchive(ModelIdRequest request,
                                                        IServerStreamWriter<ArchivePartReply> responseStream,
                                                        ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(request.ModelId);
            }
            catch (Exception exception)
            {
                ThrowUnknownException(exception);
            }
            if (model == null)
            {
                string exceptionMessage = $"Model with id '{request.ModelId}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            string modelDirectory = ModelsDirectory + model.Id;
            string modelZipFilePath = TmpDirectory + model.Id + "-" + Guid.NewGuid().ToString();
            try
            {
                ZipFile.CreateFromDirectory(modelDirectory, modelZipFilePath);
                byte[] buffer = new byte[BufferSize];
                using (FileStream fs = File.OpenRead(modelZipFilePath))
                {
                    long bytesLeft = fs.Length;
                    while (bytesLeft > 0)
                    {
                        int bytesRead = fs.Read(buffer, 0, BufferSize);
                        bytesLeft -= bytesRead;
                        ArchivePartReply archivePart = new ArchivePartReply
                        {
                            Data = ByteString.CopyFrom(buffer),
                            BytesToRead = bytesRead
                        };
                        await responseStream.WriteAsync(archivePart);
                    }
                }
            }
            catch (Exception exception)
            {
                File.Delete(modelZipFilePath);
                ThrowUnknownException(exception);
            }
            File.Delete(modelZipFilePath);
        }

        // method for DSS
        public async Task<ModelConfiguration> GetModelConfigurationInternal(string modelId)
        {
            ModelConfiguration configuration = null;
            try
            {
                Model? model = await ModelsRepository.GetById(modelId);
                configuration = ModelConfiguration.FromJsonDocument(model.Configuration);
            }
            catch (Exception exception)
            {
                ThrowInternalException(exception);
            }
            return configuration;
        }
    }
}
