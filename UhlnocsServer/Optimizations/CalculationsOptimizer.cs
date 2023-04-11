using System.Text.Json;
using UhlnocsServer.Calculations;
using UhlnocsServer.Database;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Infos;
using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Services;
using static UhlnocsServer.Utils.ExceptionUtils;
using static UhlnocsServer.Utils.PropertiesHolder;

namespace UhlnocsServer.Optimizations
{
    public class CalculationsOptimizer
    {
        private readonly IRepository<Model> ModelsRepository;
        private readonly IRepository<Launch> LaunchesRepository;
        private readonly IRepository<Calculation> CalculationsRepository;
        private readonly IRepository<ParametersSet> ParametersRepository;
        private readonly IRepository<CharacteristicsSet> CharacteristicsRepository;

        public readonly string TmpDirectory = ModelSettings.GetValue<string>("tmpDirectory");
        public readonly string ModelsDirectory = ModelSettings.GetValue<string>("modelsDirectory");

        public CalculationsOptimizer(IRepository<Model> modelsRepository,
                                     IRepository<Launch> launchesRepository,
                                     IRepository<Calculation> calculationsRepository,
                                     IRepository<ParametersSet> parametersRepository,
                                     IRepository<CharacteristicsSet> characteristicsRepository)
        {
            ModelsRepository = modelsRepository;
            LaunchesRepository = launchesRepository;
            CalculationsRepository = calculationsRepository;
            ParametersRepository = parametersRepository;
            CharacteristicsRepository = characteristicsRepository;
        }

        // TODO: LaunchConfiguration class contains more information than model needs
        // TODO: think about try-catches
        public async Task OptimizeLaunch(LaunchConfiguration launchConfiguration)
        {
            Launch launch = new Launch
            {
                Id = Guid.NewGuid().ToString(),
                Name = launchConfiguration.Name,
                Description = launchConfiguration.Description,
                UserId = launchConfiguration.User,
                UserParameters = JsonDocument.Parse(JsonSerializer.Serialize(launchConfiguration.UserParameters)),
                UserCharacteristics = JsonDocument.Parse(JsonSerializer.Serialize(launchConfiguration.UserCharacteristics)),
                OptimizationAlgorithm = JsonDocument.Parse(OptimizationAlgorithm.ToJsonString(launchConfiguration.OptimizationAlgorithm)),
                RecalculateExisting = launchConfiguration.RecalculateExisting,
                SearchAccuracy = launchConfiguration.SearchAccuracy,
                Status = LaunchStatus.Running,
                StartTime = DateTime.Now,
                EndTime = null,
                Duration = null
            };
            try
            {
                await LaunchesRepository.Create(launch);
            }
            catch (Exception exception)
            {
                ThrowInternalException(exception);
            }

            List<string> modelsIds = new();
            foreach (CharacteristicWithModel characteristic in launchConfiguration.Characteristics)
            {
                modelsIds.Add(characteristic.Id);
            }

            Dictionary<string, List<ParameterValue>> parametersOfModels = new();
            foreach (string modelId in modelsIds)
            {
                parametersOfModels[modelId] = new();
            }
            foreach (ParameterValue parameter in launchConfiguration.Parameters)
            {
                List<string> modelsThatCanCalculateParameter = ModelService.ParametersWithModels[parameter.Id].Models;
                foreach (string modelId in modelsIds)
                {
                    if (modelsThatCanCalculateParameter.Contains(modelId))
                    {
                        parametersOfModels[modelId].Add(parameter);
                    }
                }
            }

            Dictionary<string, List<CharacteristicWithModel>> characteristicsOfModels = new();
            foreach (string modelId in modelsIds)
            {
                characteristicsOfModels[modelId] = new();
            }
            foreach (CharacteristicWithModel characteristic in launchConfiguration.Characteristics)
            {
                characteristicsOfModels[characteristic.Model].Add(characteristic);
            }

            Task[] modelsTasks = new Task[modelsIds.Count];
            for (int i = 0; i < modelsIds.Count; ++i)
            {
                LaunchConfiguration modelLaunchConfiguration = new LaunchConfiguration(launchConfiguration.Name,
                                                                                       launchConfiguration.Description,
                                                                                       launchConfiguration.User,
                                                                                       parametersOfModels[modelsIds[i]],
                                                                                       launchConfiguration.UserParameters,
                                                                                       characteristicsOfModels[modelsIds[i]],
                                                                                       launchConfiguration.UserCharacteristics,
                                                                                       launchConfiguration.OptimizationAlgorithm,
                                                                                       launchConfiguration.RecalculateExisting,
                                                                                       launchConfiguration.SearchAccuracy);
                Task modelTask = Task.Run(() => OptimizeModel(launch.Id, modelsIds[i], modelLaunchConfiguration));
                modelsTasks[i] = modelTask;
            }
            Task.WaitAll(modelsTasks);

            launch.EndTime = DateTime.Now;
            launch.Status = LaunchStatus.Finished;
            try
            {
                await LaunchesRepository.Update(launch);
            }
            catch (Exception exception)
            {
                ThrowInternalException(exception);
            }
        }

        // TODO: think about try-catches
        public async Task OptimizeModel(string launchId, string modelId, LaunchConfiguration modelLaunchConfiguration)
        {
            Task[] calculationsTasks = null;
            string variableParameterId = modelLaunchConfiguration.OptimizationAlgorithm.VariableParameter;
            ParameterValue variableParameter = modelLaunchConfiguration.GetParameterValue(variableParameterId);
            PropertyValueType valueType = ModelService.ParametersWithModels[variableParameterId].ValueType;

            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(modelId);
            }
            catch (Exception exception)
            {
                ThrowInternalException(exception);
            }
            ModelConfiguration modelConfiguration = ModelConfiguration.FromJsonDocument(model.Configuration);

            if (modelLaunchConfiguration.OptimizationAlgorithm is ConstantStep constantStep)
            {
                if (valueType == PropertyValueType.Bool)
                {
                    constantStep.Step = 1;
                    constantStep.Iterations = 2;
                }
                else if (valueType == PropertyValueType.String)
                {
                    constantStep.Step = 1;
                    StringParameterInfo parameterInfo = modelConfiguration.GetParameterInfo<StringParameterInfo>(variableParameterId);
                    constantStep.Iterations = parameterInfo.PossibleValues.Count;
                }
                calculationsTasks = new Task[constantStep.Iterations];

                for (int i = 0; i < constantStep.Iterations; ++i)
                {
                    List<ParameterValue> calculationParameters = new();
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
                            if (valueType == PropertyValueType.String)
                            {
                                StringParameterInfo parameterInfo = modelConfiguration.GetParameterInfo<StringParameterInfo>(variableParameterId);
                                string variableParameterValue = parameterInfo.PossibleValues[i];
                                calculationParameters.Add(new StringParameterValue(variableParameterId, variableParameterValue));
                            }
                            else if (valueType == PropertyValueType.Bool)
                            {
                                bool variableParameterValue = (i == 0) ? true : false;
                                calculationParameters.Add(new BoolParameterValue(variableParameterId, variableParameterValue));
                            }
                            else if (valueType == PropertyValueType.Double)
                            {
                                double firstValue = (variableParameter as DoubleParameterValue).Value;
                                double variableParameterValue = firstValue + constantStep.Step * i;
                                calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                            }
                            else  // int
                            {
                                int firstValue = (variableParameter as IntParameterValue).Value;
                                int variableParameterValue = firstValue + (int)Math.Round(constantStep.Step * i);
                                calculationParameters.Add(new IntParameterValue(variableParameterId, variableParameterValue));
                            }
                        }
                    }
                    Task calculationTask = Task.Run(() => OptimizeCalculation(launchId, modelConfiguration, calculationParameters));
                    calculationsTasks[i] = calculationTask;
                }
                Task.WaitAll(calculationsTasks);
            }
            else if (modelLaunchConfiguration.OptimizationAlgorithm is SmartConstantStep smartConstantStep)
            {
                int iteration = 0;
                bool noCalculationError = true;
                double variableParameterValue = 0;
                double throughputCharacteristicValue = 0;
                do
                {
                    
                    List<ParameterValue> calculationParameters = new();
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
                            double firstValue = (variableParameter as DoubleParameterValue).Value;
                            variableParameterValue = firstValue + smartConstantStep.Step * iteration;
                            calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                        }
                    }

                    
                    List<CharacteristicValue> calculationCharacteristics = await OptimizeCalculation(launchId, modelConfiguration, calculationParameters);
                    if (calculationCharacteristics == null)
                    {
                        noCalculationError = false;
                    }
                    else
                    {
                        throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics,
                                                                                               smartConstantStep.ThroughputCharacteristic);
                    }
                    
                    ++iteration;
                }
                while (noCalculationError &&
                       iteration < smartConstantStep.MaxIterations && 
                       OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, smartConstantStep.Accuracy));
            }
            // else if ...

            
        }

        public async Task<List<CharacteristicValue>> OptimizeCalculation(string launchId, ModelConfiguration modelConfiguration, List<ParameterValue> parameters)
        {
            string calculationId = Guid.NewGuid().ToString();
            string modelId = modelConfiguration.Id;

            string modelPreparerFilePath = GetModelFilePath(modelId, modelConfiguration.PreparerFilePath);
            string cadFormatParametersFilePath = GetTmpFilePath(launchId, modelId, calculationId, "params_in_cad_format");
            string modelFormatParametersFilePath = GetTmpFilePath(launchId, modelId, calculationId, "params_in_model_format");
            bool noPreparerError = PrepareParameters(modelPreparerFilePath, cadFormatParametersFilePath,
                                                     modelFormatParametersFilePath, parameters);
            if (!noPreparerError)
            {
                return null;
            }


        }

        public bool PrepareParameters(string modelPreparerFilePath,
                                      string cadFormatParametersFilePath,
                                      string modelFormatParametersFilePath,
                                      List<ParameterValue> parameters)
        {

        }

        public string GetTmpFilePath(string launchId, string modelId, string calculationId, string fileName)
        {
            return $"{TmpDirectory}\\{launchId}\\{modelId}\\{fileName}";
        }

        public string GetModelFilePath(string modelId, string relativeModelFilePath)
        {
            return $"{ModelsDirectory}\\{modelId}\\{relativeModelFilePath}";
        }
    }
}
