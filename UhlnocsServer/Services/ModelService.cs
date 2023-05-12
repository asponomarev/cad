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
using UhlnocsServer.Utils;

namespace UhlnocsServer.Services
{
    public class ModelService : ModelServiceProto.ModelServiceProtoBase
    {
        private readonly ILogger<ModelService> Logger;
        private readonly IRepository<Model> ModelsRepository;
        private readonly UserService UserService;

        // take some values from properties files
        public readonly string TmpDirectory = PropertiesHolder.ModelSettings.GetValue<string>("tmpDirectory");
        public readonly string ModelsDirectory = PropertiesHolder.ModelSettings.GetValue<string>("modelsDirectory");
        public readonly int BufferSize = PropertiesHolder.ModelSettings.GetValue<int>("modelServiceBufferSize");

        // very useful dictionaries which help to resolve dependencies parameter/characteristic -> models
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
                ExceptionUtils.ThrowInternalException(exception);
            }

            // key is parameterId/characteristicId, value contains parameter name, value type and list of models
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

            //just return content of dictionary
            string parametersWithModelsJson = JsonSerializer.Serialize(ParametersWithModels, PropertyBase.PropertySerializerOptions);
            return new ParametersWithModelsReply
            {
                ParametersWithModelsJson = parametersWithModelsJson
            };
        }

        public override async Task<CharacteristicsWithModelsReply> GetCharacteristicsWithModels(ModelEmptyMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            //just return content of dictionary
            string characteristicsWithModelsJson = JsonSerializer.Serialize(CharacteristicsWithModels, PropertyBase.PropertySerializerOptions);
            return new CharacteristicsWithModelsReply
            {
                CharacteristicsWithModelsJson = characteristicsWithModelsJson
            };
        }

        public override async Task<ModelEmptyMessage> AddModelConfiguration(ModelConfigurationRequest request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            // get model configuration from request
            JsonDocument configurationJsonDocument = null;
            ModelConfiguration configuration = null;
            try
            {
                configurationJsonDocument = JsonDocument.Parse(request.ModelConfigurationJson);
                configuration = ModelConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception) 
            {
                // bad request because we get exception here only if something is wrong with configuration in request
                ExceptionUtils.ThrowBadRequestException(exception);
            }

            // create new model, add (insert) it to database
            Model model = new(configuration.Id, configurationJsonDocument);
            try
            {
                await ModelsRepository.Create(model);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowUnknownException(exception);
            }

            // adjust content of dictionaries
            SetPropertiesInfoWithModels();

            return new ModelEmptyMessage
            {

            };
        }

        public override async Task<ModelConfigurationReply> GetModelConfiguration(ModelIdRequest request,  ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            // trying to find model
            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(request.ModelId);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowUnknownException(exception);
            }

            if (model == null)
            {
                string exceptionMessage = $"Model with id '{request.ModelId}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            // return configuration and performance
            string modelConfigurationJson = JsonSerializer.Serialize(model.Configuration);
            return new ModelConfigurationReply
            {
                ModelConfigurationJson = modelConfigurationJson,
                Performance = model.Performance
            };
        }

        public override async Task<ModelsConfigurationsReply> GetModelsConfigurations(ModelEmptyMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            List<Model> models = new();
            try
            {
                models = ModelsRepository.Get().ToList();
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowUnknownException(exception);
            }

            // return configs and performances of all known models
            ModelsConfigurationsReply modelsConfigurationsReply = new() { };
            foreach (Model model in models)
            {
                string modelConfigurationJson = JsonSerializer.Serialize(model.Configuration);
                ModelConfigurationReply modelConfigurationReply = new ModelConfigurationReply
                {
                    ModelConfigurationJson = modelConfigurationJson,
                    Performance = model.Performance
                };
                modelsConfigurationsReply.ModelsConfigurationsReplies.Add(modelConfigurationReply);
            }

            return modelsConfigurationsReply;
        }

        public override async Task<ModelEmptyMessage> UpdateModelConfiguration(ModelConfigurationRequest request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            // checking request
            JsonDocument newConfigurationJsonDocument = null;
            ModelConfiguration newConfiguration = null;
            try
            {
                newConfigurationJsonDocument = JsonDocument.Parse(request.ModelConfigurationJson);
                newConfiguration = ModelConfiguration.FromJsonDocument(newConfigurationJsonDocument);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowBadRequestException(exception);
            }

            // trying to find model
            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(newConfiguration.Id);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowUnknownException(exception);
            }
            if (model == null)
            {
                string exceptionMessage = $"Model with id '{newConfiguration.Id}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            // checking permissions
            ModelConfiguration oldConfiguration = ModelConfiguration.FromJsonDocument(model.Configuration);
            if (!oldConfiguration.ResponsibleUsers.Contains(sender.Id) && UserService.IsNotAdmin(sender.Id))
            {
                string exceptionMessage = $"User with id '{sender.Id}' is not allowed to update model with id '{oldConfiguration.Id}'";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            // updating model
            model.Configuration = newConfigurationJsonDocument;
            try
            {
                await ModelsRepository.Update(model);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowUnknownException(exception);
            }

            // adjusting content of dictionaries
            SetPropertiesInfoWithModels();

            return new ModelEmptyMessage
            {

            };
        }

        public override async Task<ModelEmptyMessage> DeleteModel(ModelIdRequest request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            // we can archive models, but we don't want to delete them and info about them entirely
            if (UserService.IsNotAdmin(sender.Id))
            {
                string exceptionMessage = "Model deletion is considered an unsafe operation and only admins may perform it";
                throw new RpcException(new Status(StatusCode.PermissionDenied, exceptionMessage));
            }

            // delete model configuration and model files
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
                ExceptionUtils.ThrowUnknownException(exception);
            }

            // adjusting content of dictionaries
            SetPropertiesInfoWithModels();

            return new ModelEmptyMessage
            {

            };
        }

        public override async Task<ModelEmptyMessage> UploadModelArchive(IAsyncStreamReader<ArchivePartRequest> requestStream,
                                                                         ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            string modelDirectory = string.Empty;
            string modelZipFilePath = string.Empty;
            byte[] buffer = new byte[BufferSize];
            FileStream fs = null;
            bool first = true;

            // this method allows to upload archive with model files from client to server
            await foreach (ArchivePartRequest archivePart in requestStream.ReadAllAsync())
            {
                // if it is first part of data create directory and stream
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
                        ExceptionUtils.ThrowUnknownException(exception);
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
                            ExceptionUtils.ThrowUnknownException(exception);
                        }
                    }

                    modelZipFilePath = TmpDirectory + model.Id + "-" + Guid.NewGuid().ToString();
                    fs = File.OpenWrite(modelZipFilePath);
                }

                // read data part
                buffer = archivePart.Data.ToByteArray();
                int bytesToRead = archivePart.BytesToRead;
                try
                {
                    fs.Write(buffer, 0, bytesToRead);
                }
                catch (Exception exception)
                {
                    fs.Close();
                    ExceptionUtils.ThrowUnknownException(exception);
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
                ExceptionUtils.ThrowUnknownException(exception);
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
                ExceptionUtils.ThrowUnknownException(exception);
            }
            if (model == null)
            {
                string exceptionMessage = $"Model with id '{request.ModelId}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            // this method allows to download model archive from server to client
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
                ExceptionUtils.ThrowUnknownException(exception);
            }
            File.Delete(modelZipFilePath);
        }

        // method for DSS that returns model configuration
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
                ExceptionUtils.ThrowInternalException(exception);
            }
            return configuration;
        }

        // method for DSS that returns model performance
        public Task<double> GetModelPerformance(string modelId)
        {
            double performance = 0;
            try
            {
                performance = ModelsRepository.Get()
                    .Where(m => m.Id == modelId)
                    .Select(m => m.Performance)
                    .FirstOrDefault();
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowInternalException(exception);
            }
            return Task.FromResult(performance);
        }
    }
}
