using System.Diagnostics;
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
        private const string PreparerAndCollectorAgrumentsFormatString = "--in_file_path {0} --out_file_path {1}";
        private const string LogFileName = "log";

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

        // method for DSS
        public Task<List<List<ParameterValue>>> GetModelParametersValues(string modelId)
        {
            List<List<ParameterValue>> modelParametersValues = new();
            try
            {
                /*
                 * SELECT parameters_sets.parameters_values
                 * FROM parameters_sets
                 * INNER JOIN calculations
                 * ON calculations.parameters_hash = parameters_sets.hash
                 * WHERE calculations.model_id = modelId;
                 */
                List<JsonDocument> parametersValuesDocuments = (from ps in ParametersRepository.Get()
                                                                join c in CalculationsRepository.Get()
                                                                on ps.Hash equals c.ParametersHash
                                                                where c.ModelId == modelId
                                                                select ps.ParametersValuesJson).ToList();

                foreach (JsonDocument parametersValuesDocument in parametersValuesDocuments)
                {
                    modelParametersValues.Add(ParameterValue.ListFromJsonDocument(parametersValuesDocument));
                }
            }
            catch (Exception exception) 
            {
                ThrowInternalException(exception);            
            }
            return Task.FromResult(modelParametersValues);
        }

        public async Task OptimizeLaunch(LaunchConfiguration launchConfiguration)
        {
            // create launch
            Launch launch = await CreateLaunch(launchConfiguration);
            if (launch == null)
            {
                return;
            }

            // get ids of models which will be used to calculate characteristics in this launch
            List<string> modelsIds = new();
            foreach (CharacteristicWithModel characteristic in launchConfiguration.Characteristics)
            {
                modelsIds.Add(characteristic.Model);
            }

            // make sublists of parameters for each model from list that contains all parameters for all models
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

            // create, start and wait models tasks
            Task<ModelStatus>[] modelsTasks = new Task<ModelStatus>[modelsIds.Count];
            for (int i = 0; i < modelsIds.Count; ++i)
            {
                Task<ModelStatus> modelTask = Task.Run(() => OptimizeModel(launch.Id,
                                                                           modelsIds[i],
                                                                           parametersOfModels[modelsIds[i]],
                                                                           launchConfiguration.OptimizationAlgorithm));
                modelsTasks[i] = modelTask;
            }
            Task.WaitAll(modelsTasks);

            // update launch
            LaunchStatus launchStatus = GetLaunchStatus(modelsTasks);
            await OnLaunchFinished(launch, launchStatus);
        }

        // will be beautified later
        public async Task<ModelStatus> OptimizeModel(string launchId,
                                                     string modelId,
                                                     List<ParameterValue> parameters,
                                                     OptimizationAlgorithm optimizationAlgorithm)
        {
            Task<List<CharacteristicValue>>[] calculationsTasks = null;
            string variableParameterId = optimizationAlgorithm.VariableParameter;
            ParameterValue variableParameter = ParameterValue.GetFromListById(parameters, variableParameterId);
            PropertyValueType valueType = ModelService.ParametersWithModels[variableParameterId].ValueType;

            Model? model = null;
            try
            {
                model = await ModelsRepository.GetById(modelId);
            }
            catch (Exception exception)
            {
                string LogFilePath = GetModelTmpFilePath(launchId, modelId, LogFileName);
                string message = "Unable to get model due to database error!" + Environment.NewLine +
                                 GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(LogFilePath, message);
                return ModelStatus.FinishedAllFailed;
            }
            ModelConfiguration modelConfiguration = ModelConfiguration.FromJsonDocument(model.Configuration);

            if (optimizationAlgorithm is ConstantStep constantStep)
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
                calculationsTasks = new Task<List<CharacteristicValue>>[constantStep.Iterations];

                for (int i = 0; i < constantStep.Iterations; ++i)
                {
                    List<ParameterValue> calculationParameters = new();
                    foreach (ParameterValue parameter in parameters)
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
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId, modelConfiguration, calculationParameters));
                    calculationsTasks[i] = calculationTask;
                }
                Task.WaitAll(calculationsTasks);
            }
            else if (optimizationAlgorithm is SmartConstantStep smartConstantStep)
            {
                int iteration = 0;
                bool noCalculationError = true;
                double variableParameterValue = 0;
                double throughputCharacteristicValue = 0;
                do
                {
                    
                    List<ParameterValue> calculationParameters = new();
                    foreach (ParameterValue parameter in parameters)
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

            return GetModelStatus(calculationsTasks);
        }

        public async Task<List<CharacteristicValue>> OptimizeCalculation(string launchId,
                                                                         ModelConfiguration modelConfiguration,
                                                                         List<ParameterValue> parameters)
        {          
            string modelId = modelConfiguration.Id;

            // CREATE PARAMETERS
            string parametersHash = await CreateParameters(parameters, modelId, launchId);
            if (parametersHash == null)
            {
                return null;
            }

            // CREATE CALCULATION
            Calculation calculation = await CreateCalculation(launchId, modelId, parametersHash);
            if (calculation == null)
            {
                return null;
            }
            string calculationId = calculation.Id;

            // PREPARE PARAMETERS
            string modelPreparerFilePath = GetModelExecutableFilePath(modelId, modelConfiguration.PreparerFilePath);
            string cadFormatParametersFilePath = GetCalculationTmpFilePath(launchId, modelId, calculationId, "params_in_cad_format");
            string modelFormatParametersFilePath = GetCalculationTmpFilePath(launchId, modelId, calculationId, "params_in_model_format");
            bool noPreparerError = await PrepareParameters(modelPreparerFilePath, modelConfiguration.PreparerOkExitCode,
                                                           cadFormatParametersFilePath, modelFormatParametersFilePath,
                                                           parameters, calculation);
            if (!noPreparerError) 
            {
                return null;
            }

            // RUN MODEL
            string modelExecutableFilePath = GetModelExecutableFilePath(modelId, modelConfiguration.ModelFilePath);
            string modelFormatCharacteristicsFilePath = GetCalculationTmpFilePath(launchId, modelId, calculationId, "characteristics_in_model_format");
            bool noModelError = await RunModel(modelExecutableFilePath, modelConfiguration.ModelArgumentsFormatString,
                                               modelConfiguration.CollectFromStdout, modelConfiguration.ModelOkExitCode,
                                               modelFormatParametersFilePath, modelFormatCharacteristicsFilePath, calculation);
            if (!noModelError)
            {
                return null;
            }

            // COLLECT, READ AND CREATE CHARACTERISTICS
            string modelCollectorFilePath = GetModelExecutableFilePath(modelId, modelConfiguration.CollectorFilePath);
            string cadFormatCharacteristicsFilePath = GetCalculationTmpFilePath(launchId, modelId, calculationId, "characteristics_in_cad_format");
            return await CollectCharacteristics(modelCollectorFilePath, modelConfiguration.CollectorOkExitCode,
                                                modelFormatCharacteristicsFilePath, cadFormatCharacteristicsFilePath,
                                                calculation);
        }

        private async Task<Launch> CreateLaunch(LaunchConfiguration launchConfiguration)
        {
            Launch launch = new()
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
            bool noCreateLaunchError = true;
            try
            {
                await LaunchesRepository.Create(launch);
            }
            catch (Exception exception)
            {
                string LogFilePath = GetLaunchTmpFilePath(launch.Id, LogFileName);
                string message = "Unable to create launch due to database error!" + Environment.NewLine +
                                 GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(LogFilePath, message);
                noCreateLaunchError = false;
            }
            return (noCreateLaunchError) ? launch : null;
        }

        private LaunchStatus GetLaunchStatus(Task<ModelStatus>[] modelsTasks)
        {
            int totalTasks = modelsTasks.Length;
            int modelsNoFailed = 0;
            int modelsAllFailed = 0;
            foreach (Task<ModelStatus> task in modelsTasks)
            {
                if (task.Result == ModelStatus.FinishedNoFailed)
                {
                    ++modelsNoFailed;
                }
                else if (task.Result == ModelStatus.FinishedAllFailed) { }
                {
                    ++modelsAllFailed;
                }
            }

            if (totalTasks == modelsNoFailed)
            {
                return LaunchStatus.FinishedNoFailed;
            }
            if (totalTasks == modelsAllFailed)
            {
                return LaunchStatus.FinishedAllFailed;
            }
            return LaunchStatus.FinishedSomeFailed;
        }

        private async Task OnLaunchFinished(Launch launch, LaunchStatus launchStatus)
        {
            launch.EndTime = DateTime.Now;
            launch.Duration = launch.EndTime - launch.StartTime;
            launch.Status = launchStatus;
            try
            {
                await LaunchesRepository.Update(launch);
            }
            catch (Exception exception)
            {
                string LogFilePath = GetLaunchTmpFilePath(launch.Id, LogFileName);
                string message = "Unable to update launch due to database error!" + Environment.NewLine +
                                 GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(LogFilePath, message);
            }
        }

        private ModelStatus GetModelStatus(Task<List<CharacteristicValue>>[] calculationsTasks)
        {
            int totalTasks = calculationsTasks.Length;
            int completedTasks = 0;
            int failedTasks = 0;
            foreach (Task<List<CharacteristicValue>> task in calculationsTasks)
            {
                if (task.Result == null)
                {
                    ++failedTasks;
                }
                else
                {
                    ++completedTasks;
                }
            }

            if (totalTasks == completedTasks)
            {
                return ModelStatus.FinishedNoFailed;
            }
            if (totalTasks == failedTasks)
            {
                return ModelStatus.FinishedAllFailed;
            }
            return ModelStatus.FinishedSomeFailed;
        }

        private async Task<bool> PrepareParameters(string preparerFilePath,
                                                  int preparerOkExitCode,
                                                  string cadFormatParametersFilePath,
                                                  string modelFormatParametersFilePath,
                                                  List<ParameterValue> parameters,
                                                  Calculation calculation)
        {
            bool noPreparerError = true;
            try
            {
                string parametersJson = ParameterValue.ListToJsonString(parameters);
                File.WriteAllText(cadFormatParametersFilePath, parametersJson);

                Process prepareProcess = ConfigureProcess(preparerFilePath,
                                                          PreparerAndCollectorAgrumentsFormatString,
                                                          new string[] { cadFormatParametersFilePath, modelFormatParametersFilePath });
                prepareProcess.Start();
                prepareProcess.WaitForExit();
                if (prepareProcess.ExitCode != preparerOkExitCode)
                {
                    throw new Exception($"Preparer process finished with not ok exit code {prepareProcess.ExitCode}");
                }
            }
            catch (Exception exception)
            {
                await OnCalculationException(calculation, exception, "Preparation stage error happened!");
                noPreparerError = false;
            }
            return noPreparerError;
        }

        private async Task<bool> RunModel(string modelFilePath,
                                          string modelArgumentsFormatString,
                                          bool collectFromStdout,
                                          int modelOkExitCode,
                                          string modelFormatParametersFilePath,
                                          string modelFormatCharacteristicsFilePath,
                                          Calculation calculation)
        {
            bool noModelError = true;
            try
            {
                string[] modelProcessArguments;
                if (collectFromStdout)
                {
                    modelProcessArguments = new string[] { modelFormatParametersFilePath };
                }
                else
                {
                    modelProcessArguments = new string[] { modelFormatParametersFilePath, modelFormatCharacteristicsFilePath };
                }
                Process modelProcess = ConfigureProcess(modelFilePath, modelArgumentsFormatString, modelProcessArguments);
                if (collectFromStdout)
                {
                    // modelProcess.StartInfo.UseShellExecute = false;
                    modelProcess.StartInfo.RedirectStandardOutput = true;
                }

                modelProcess.Start();
                string modelOutput = string.Empty;
                if (collectFromStdout)
                {
                    modelOutput = modelProcess.StandardOutput.ReadToEnd();
                }
                modelProcess.WaitForExit();

                if (collectFromStdout)
                {
                    File.WriteAllText(modelFormatCharacteristicsFilePath, modelOutput);
                }
                if (modelProcess.ExitCode != modelOkExitCode)
                {
                    throw new Exception($"Model process finished with not ok exit code {modelProcess.ExitCode}");
                }
            }
            catch (Exception exception)
            {
                await OnCalculationException(calculation, exception, "Modelling stage error happened!");
                noModelError = false;
            }
            return noModelError;
        }

        private async Task<List<CharacteristicValue>> CollectCharacteristics(string collectorFilePath,
                                                                             int collectorOkExitCode,
                                                                             string modelFormatCharacteristicsFilePath,
                                                                             string cadFormatCharacteristicsFilePath,
                                                                             Calculation calculation)
        {
            List<CharacteristicValue> characteristics = null;
            bool noCollectorError = true;
            try
            {
                Process collectProcess = ConfigureProcess(collectorFilePath,
                                                          PreparerAndCollectorAgrumentsFormatString,
                                                          new string[] { modelFormatCharacteristicsFilePath, cadFormatCharacteristicsFilePath });
                collectProcess.Start();
                collectProcess.WaitForExit();
                if (collectProcess.ExitCode != collectorOkExitCode)
                {
                    throw new Exception($"Collector process finished with not ok exit code {collectProcess.ExitCode}");
                }

                string characteristicsId = Guid.NewGuid().ToString();
                characteristics = await ReadAndCreateCharacteristics(characteristicsId, cadFormatCharacteristicsFilePath);

                await OnCalculationCompleted(calculation, characteristicsId);
            }
            catch (Exception exception)
            {
                await OnCalculationException(calculation, exception, "Collecting stage error happened!");
                noCollectorError = false;
            }
            return (noCollectorError) ? characteristics : null;
        }

        private async Task<string> CreateParameters(List<ParameterValue> parameters, string modelId, string launchId)
        {
            string parametersHash = ParameterValue.GetHashCode(parameters, modelId);
            ParametersSet parametersSet = new()
            {
                Hash = parametersHash,
                ParametersValuesJson = ParameterValue.ListToJsonDocument(parameters)
            };

            bool noCreateParametersSetError = true;
            try
            {
                await ParametersRepository.Create(parametersSet);
            }
            catch (Exception exception)
            {
                noCreateParametersSetError = false;
                string logFilePath = GetModelTmpFilePath(launchId, modelId, LogFileName);
                string message = "Unable to create parameters set due to database exception!" + Environment.NewLine
                                 + GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }

            return (noCreateParametersSetError) ? parametersHash : null;
        }

        private async Task<Calculation> CreateCalculation(string launchId, string modelId, string parametersHash)
        {
            Calculation calculation = new()
            {
                Id = Guid.NewGuid().ToString(),
                LaunchId = launchId,
                ModelId = modelId,
                ParametersHash = parametersHash,
                CharacteristicsId = null,
                ReallyCalculated = true,
                Status = CalculationStatus.Running,
                StartTime = DateTime.Now,
                EndTime = null,
                Duration = null,
                Message = null
            };

            bool noCreateCalculationError = true;
            try
            {
                await CalculationsRepository.Create(calculation);
            }
            catch (Exception exception)
            {
                noCreateCalculationError = false;
                string logFilePath = GetModelTmpFilePath(launchId, modelId, LogFileName);
                string message = "Unable to create calculation due to database exception!" + Environment.NewLine
                                 + GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }

            return (noCreateCalculationError) ? calculation : null;
        }

        private static Process ConfigureProcess(string executableFilePath, string argumentsFormatString, string[] arguments)
        {
            Process process = new();
            process.StartInfo.CreateNoWindow = true;

            if (executableFilePath.EndsWith(".exe")) {
                process.StartInfo.FileName = executableFilePath;
                process.StartInfo.Arguments = string.Format(argumentsFormatString, arguments);
            }
            else if (executableFilePath.EndsWith(".py"))
            {
                process.StartInfo.FileName = "python";
                process.StartInfo.Arguments = $"{executableFilePath} {string.Format(argumentsFormatString, arguments)}";
            }
            else if (executableFilePath.EndsWith(".jar"))
            {
                process.StartInfo.FileName = "java";
                process.StartInfo.Arguments = $"-jar {executableFilePath} {string.Format(argumentsFormatString, arguments)}";
            }
            else
            {
                throw new Exception($"Unknown type of executable file {executableFilePath}");
            }

            return process;
        }

        private async Task OnCalculationException(Calculation calculation, Exception exception, string messagePrefix)
        {
            calculation.Message = messagePrefix + Environment.NewLine + GetExceptionMessage(exception);
            calculation.EndTime = DateTime.Now;
            calculation.Duration = calculation.EndTime - calculation.StartTime;
            calculation.Status = CalculationStatus.Failed;
            try
            {
                await CalculationsRepository.Update(calculation);
            }
            catch (Exception databaseException)
            {
                string logFilePath = GetCalculationTmpFilePath(calculation.LaunchId, calculation.ModelId, calculation.Id, LogFileName);
                string message = "Unable to write information about calculation exception to database due to database exception" +
                                 Environment.NewLine + "Calculation exception info" + Environment.NewLine +
                                 calculation.Message + Environment.NewLine +
                                 "Database exception info" + Environment.NewLine +
                                 GetExceptionMessage(databaseException) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }           
        }

        private async Task<List<CharacteristicValue>> ReadAndCreateCharacteristics(string characteristicsId,
                                                                                   string cadFormatCharacteristicsFilePath)
        {
            string characteristicsJson = File.ReadAllText(cadFormatCharacteristicsFilePath);
            JsonDocument characteristicsDocument = JsonDocument.Parse(characteristicsJson);
            List<CharacteristicValue> characteristics = CharacteristicValue.ListFromJsonElement(characteristicsDocument.RootElement);
            
            CharacteristicsSet characteristicsSet = new()
            {
                Id = characteristicsId,
                CharacteristicsValuesJson = characteristicsDocument
            };
            try
            {
                await CharacteristicsRepository.Create(characteristicsSet);
            }
            catch (Exception)
            {
                throw new Exception("Error while adding characteristics to database!");
            }
            return characteristics;
        }

        private async Task OnCalculationCompleted(Calculation calculation, string characteristicsId)
        {
            calculation.CharacteristicsId = characteristicsId;
            calculation.EndTime = DateTime.Now;
            calculation.Duration = calculation.EndTime - calculation.StartTime;
            calculation.Status = CalculationStatus.Completed;
            try
            {
                await CalculationsRepository.Update(calculation);
            }
            catch (Exception)
            {
                throw new Exception("Error while updating calculation after adding characteristics!");
            }
        }

        private void SafeAppendToFile(string filePath, string message)
        {
            try
            {
                File.AppendAllText(filePath, message);
            }
            catch (Exception)
            {
                // cant do anything about this
            }           
        }

        private string GetLaunchTmpFilePath(string launchId, string fileName)
        {
            return $"{TmpDirectory}\\{launchId}\\{fileName}";
        }

        private string GetModelTmpFilePath(string launchId, string modelId, string fileName)
        {
            return $"{TmpDirectory}\\{launchId}\\{modelId}\\{fileName}";
        }

        private string GetCalculationTmpFilePath(string launchId, string modelId, string calculationId, string fileName)
        {
            return $"{TmpDirectory}\\{launchId}\\{modelId}\\{calculationId}\\{fileName}";
        }

        private string GetModelExecutableFilePath(string modelId, string relativeModelFilePath)
        {
            return $"{ModelsDirectory}\\{modelId}\\{relativeModelFilePath}";
        }
    }
}
