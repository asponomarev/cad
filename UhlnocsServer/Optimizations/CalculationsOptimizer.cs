using System.Data.Entity;
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
using UhlnocsServer.Utils;

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

        public readonly string TmpDirectory = PropertiesHolder.ModelSettings.GetValue<string>("tmpDirectory");
        public readonly string ModelsDirectory = PropertiesHolder.ModelSettings.GetValue<string>("modelsDirectory");

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

        // method for DSS that returns all parameters sets for specified model
        public Task<List<List<ParameterValue>>> GetModelParametersValues(string modelId)
        {
            List<List<ParameterValue>> modelParametersValues = new();
            try
            {
                List<JsonDocument>  parametersValuesDocuments = CalculationsRepository.Get()
                                                                    .Where(c => c.ModelId == modelId)
                                                                    .Include(c => c.CharacteristicsSet)
                                                                    .Select(c => c.CharacteristicsSet.CharacteristicsValuesJson)
                                                                    .ToList();

                foreach (JsonDocument parametersValuesDocument in parametersValuesDocuments)
                {
                    modelParametersValues.Add(ParameterValue.ListFromJsonDocument(parametersValuesDocument));
                }
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowInternalException(exception);
            }
            return Task.FromResult(modelParametersValues);
        }

        // method for DSS that calculates performance of specified model
        // for now model performance is calculated as 1 / average calculations duration in ms
        public async Task RecalculateModelPerformance(string modelId)
        {
            // .Average(c => c.Duration) does not want to work because duration is interval in psql
            List<TimeSpan?> modelCalculationsDurations = CalculationsRepository.Get()
                                                            .Where(c => c.ModelId == modelId &&
                                                                        c.ReallyCalculated == true &&
                                                                        c.Status == CalculationStatus.Completed)
                                                            .Select(c => c.Duration)
                                                            .ToList();
            int totalCalculations = modelCalculationsDurations.Count;
            double totalCalculationsDuration = 0;
            foreach (TimeSpan duration in modelCalculationsDurations)  // duration can not be null if status = completed
            {
                totalCalculationsDuration += duration.TotalMilliseconds;
            }
            double averageCalculationsDuration = totalCalculationsDuration / totalCalculations;
            double performance = 1.0 / averageCalculationsDuration;

            Model? model = await ModelsRepository.GetById(modelId);
            model.Performance = performance;
            await ModelsRepository.Update(model);
        }

        public async Task OptimizeLaunch(string launchId, LaunchConfiguration launchConfiguration)
        {
            // create launch
            Launch launch = await CreateLaunch(launchId, launchConfiguration);
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
            Task<ModelAndAlgorithmStatuses>[] modelsTasks = new Task<ModelAndAlgorithmStatuses>[modelsIds.Count];
            for (int i = 0; i < modelsIds.Count; ++i)
            {
                string modelId = modelsIds[i];
                List<ParameterValue> parameters = parametersOfModels[modelId];
                Task<ModelAndAlgorithmStatuses> modelTask = Task.Run(() => OptimizeModel(launchId,
                                                                                         modelId,
                                                                                         parameters,
                                                                                         launchConfiguration.OptimizationAlgorithm,
                                                                                         launchConfiguration.RecalculateExisting));
                modelsTasks[i] = modelTask;
            }
            Task.WaitAll(modelsTasks);

            // update launch
            await OnLaunchFinished(launch, modelsTasks);
        }

        public async Task<ModelAndAlgorithmStatuses> OptimizeModel(string launchId,
                                                                   string modelId,
                                                                   List<ParameterValue> parameters,
                                                                   OptimizationAlgorithm optimizationAlgorithm,
                                                                   bool recalculateExisting)
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
                                 ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(LogFilePath, message);
                return new ModelAndAlgorithmStatuses(modelId, ModelStatus.FinishedAllFailed, AlgorithmStatus.Undefined);
            }
            ModelConfiguration modelConfiguration = ModelConfiguration.FromJsonDocument(model.Configuration);

            AlgorithmStatus algorithmStatus = AlgorithmStatus.Undefined;
            /* Constant Step Search */
            if (optimizationAlgorithm is ConstantStep constantStep)
            {
                StringParameterInfo parameterInfo = null;
                if (valueType == PropertyValueType.Bool)
                {
                    constantStep.Iterations = 2;
                }
                else if (valueType == PropertyValueType.String)
                {
                    parameterInfo = modelConfiguration.GetStringParameterInfo(variableParameterId);
                    constantStep.Iterations = parameterInfo.PossibleValues.Count;
                }
                calculationsTasks = new Task<List<CharacteristicValue>>[constantStep.Iterations];

                for (int i = 0; i < constantStep.Iterations; ++i)
                {
                    List<ParameterValue> calculationParameters = constantStep.MakeCalculationParameters(parameters, variableParameterId, i, valueType, variableParameter, parameterInfo);

                    int fixedIteration = i;  // we should not pass reference to i to the method below
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId,
                                                                                                         modelConfiguration,
                                                                                                         calculationParameters,
                                                                                                         recalculateExisting,
                                                                                                         fixedIteration));
                    calculationsTasks[i] = calculationTask;
                }
                Task.WaitAll(calculationsTasks);
            }

            /* Smart Constant Step Search */
            else if (optimizationAlgorithm is SmartConstantStep smartConstantStep)
            {
                calculationsTasks = new Task<List<CharacteristicValue>>[smartConstantStep.MaxIterations]; 
                smartConstantStep.FirstValue = ((DoubleParameterValue)variableParameter).Value;
                int iteration = 0;
                algorithmStatus = AlgorithmStatus.Calculating;

                do
                {
                    List<ParameterValue> calculationParameters = smartConstantStep.MakeCalculationParameters(parameters, variableParameterId, iteration);
                    int fixedIteration = iteration;  // we should not pass reference to iteration to the method below
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId,
                                                                                                         modelConfiguration, 
                                                                                                         calculationParameters,
                                                                                                         recalculateExisting,
                                                                                                         fixedIteration));
                    calculationsTasks[iteration] = calculationTask;
                    List<CharacteristicValue> calculationCharacteristics = await calculationTask;

                    if (calculationCharacteristics == null)
                    {
                        algorithmStatus = AlgorithmStatus.Undefined;
                        break;
                    }
                    else
                    {
                        double throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, smartConstantStep.ThroughputCharacteristic);
                        algorithmStatus = smartConstantStep.CheckPointIsGood(throughputCharacteristicValue, iteration);
                    }
                    ++iteration;
                    if (iteration == smartConstantStep.MaxIterations)
                    {
                        algorithmStatus = AlgorithmStatus.ReachedMaxIteration;
                    }
                }
                while (algorithmStatus == AlgorithmStatus.Calculating);
            }

            /* Binary Search */
            else if (optimizationAlgorithm is BinarySearch binarySearch)
            {
                calculationsTasks = new Task<List<CharacteristicValue>>[binarySearch.Iterations];
                binarySearch.FirstValue = ((DoubleParameterValue)variableParameter).Value;
                algorithmStatus = AlgorithmStatus.Calculating;

                for (int i = 0; i < binarySearch.Iterations; ++i)
                {
                    List<ParameterValue> calculationParameters = binarySearch.MakeCalculationParameters(parameters, variableParameterId, i);
                    int fixedIteration = i;  // we should not pass reference to i to the method below
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId,
                                                                                                         modelConfiguration, 
                                                                                                         calculationParameters,
                                                                                                         recalculateExisting,
                                                                                                         fixedIteration));
                    calculationsTasks[i] = calculationTask;
                    List<CharacteristicValue> calculationCharacteristics = await calculationTask;                   
                   
                    if (calculationCharacteristics == null)
                    {
                        algorithmStatus = AlgorithmStatus.Undefined;
                        break;
                    }
                    else
                    {
                        double throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics,
                                                                                                      binarySearch.ThroughputCharacteristic);
                        algorithmStatus = binarySearch.MoveBorder(throughputCharacteristicValue, i);
                        if (algorithmStatus == AlgorithmStatus.FirstPointIsBad || algorithmStatus == AlgorithmStatus.LastPointIsGood)
                        {
                            break;                        
                        }
                    }
                }
            }

            /* Smart Binary Search */
            else if (optimizationAlgorithm is SmartBinarySearch smartBinarySearch)
            {
                calculationsTasks = new Task<List<CharacteristicValue>>[smartBinarySearch.MaxIterations];
                smartBinarySearch.FirstValue = ((DoubleParameterValue)variableParameter).Value;
                algorithmStatus = AlgorithmStatus.Calculating;
                int iteration = 0;

                do
                {
                    List<ParameterValue> calculationParameters = smartBinarySearch.MakeCalculationParameters(parameters, variableParameterId, iteration);
                    int fixedIteration = iteration;  // we should not pass reference to iteration to the method below
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId, 
                                                                                                         modelConfiguration, 
                                                                                                         calculationParameters,
                                                                                                         recalculateExisting,
                                                                                                         fixedIteration));
                    calculationsTasks[iteration] = calculationTask;
                    List<CharacteristicValue> calculationCharacteristics = await calculationTask;
                    
                    if (calculationCharacteristics == null)
                    {
                        algorithmStatus = AlgorithmStatus.Undefined;
                        break;
                    }
                    else
                    {
                        double throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, 
                                                                                                      smartBinarySearch.ThroughputCharacteristic);
                        algorithmStatus = smartBinarySearch.MoveBorder(throughputCharacteristicValue, iteration);
                    }
                    ++iteration;
                    if (iteration == smartBinarySearch.MaxIterations)
                    {
                        algorithmStatus = AlgorithmStatus.ReachedMaxIteration;
                    }
                }
                while (algorithmStatus == AlgorithmStatus.Calculating);

            }

            /* Golden Section Search */
            else if (optimizationAlgorithm is GoldenSection goldenSection)
            {
                calculationsTasks = new Task<List<CharacteristicValue>>[goldenSection.Iterations];
                goldenSection.FirstValue = ((DoubleParameterValue)variableParameter).Value;
                algorithmStatus = AlgorithmStatus.Calculating;

                for (int i = 0; i < goldenSection.Iterations; ++i)
                {                 
                    List<ParameterValue> calculationParameters = goldenSection.MakeCalculationParameters(parameters, variableParameterId, i);
                    int fixedIteration = i;  // we should not pass reference to i to the method below
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId,
                                                                                                         modelConfiguration,
                                                                                                         calculationParameters,
                                                                                                         recalculateExisting,
                                                                                                         fixedIteration));
                    calculationsTasks[i] = calculationTask;
                    List<CharacteristicValue> calculationCharacteristics = await calculationTask;

                    if (calculationCharacteristics == null)
                    {
                        algorithmStatus = AlgorithmStatus.Undefined;
                        break;
                    }
                    else
                    {
                        double throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, 
                                                                                                      goldenSection.ThroughputCharacteristic);
                        algorithmStatus = goldenSection.MoveBorder(throughputCharacteristicValue, i);
                        if (algorithmStatus == AlgorithmStatus.FirstPointIsBad || algorithmStatus == AlgorithmStatus.LastPointIsGood)
                        {
                            break;                        
                        }                               
                    }
                }
            }

            /* Smart Golden Section Search */
            else if (optimizationAlgorithm is SmartGoldenSection smartGoldenSection)
            {
                calculationsTasks = new Task<List<CharacteristicValue>>[smartGoldenSection.MaxIterations];
                smartGoldenSection.FirstValue = ((DoubleParameterValue)variableParameter).Value;
                algorithmStatus = AlgorithmStatus.Calculating;
                int iteration = 0;

                do
                {
                    List<ParameterValue> calculationParameters = smartGoldenSection.MakeCalculationParameters(parameters, variableParameterId, iteration);
                    int fixedIteration = iteration;  // we should not pass reference to iteration to the method below
                    Task<List<CharacteristicValue>> calculationTask = Task.Run(() => OptimizeCalculation(launchId, 
                                                                                                         modelConfiguration,
                                                                                                         calculationParameters,
                                                                                                         recalculateExisting,
                                                                                                         iteration));
                    calculationsTasks[iteration] = calculationTask;
                    List<CharacteristicValue> calculationCharacteristics = await calculationTask;
                    if (calculationCharacteristics == null)
                    {
                        break;
                    }
                    else
                    {
                        double throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, 
                                                                                                        smartGoldenSection.ThroughputCharacteristic);
                        algorithmStatus = smartGoldenSection.MoveBorder(throughputCharacteristicValue, iteration);
                    }
                    ++iteration;
                    if (iteration == smartGoldenSection.MaxIterations)
                    {
                        algorithmStatus = AlgorithmStatus.ReachedMaxIteration;
                    }
                }
                while (algorithmStatus == AlgorithmStatus.Calculating);
            }
          
            return new ModelAndAlgorithmStatuses(modelId, GetModelStatus(calculationsTasks), algorithmStatus);
        }

        public async Task<List<CharacteristicValue>> OptimizeCalculation(string launchId,
                                                                         ModelConfiguration modelConfiguration,
                                                                         List<ParameterValue> parameters,
                                                                         bool recalculateExisting,
                                                                         int iterationIndex)
        {          
            string modelId = modelConfiguration.Id;

            // CREATE PARAMETERS if necessary
            string parametersHash = await CreateParametersSetIfNotExists(parameters, modelId, launchId);
            if (parametersHash == null)
            {
                return null;
            }

            // TRYING TO FIND CHARACTERISTICS FOR THIS PARAMETERS SET
            if (!recalculateExisting)
            {
                CharacteristicsSet? characteristicsSet = FindCharacteristics(launchId, modelId, parametersHash);
                // FOUND CHARACTERISTICS
                if (characteristicsSet != null)
                {
                    // CREATE FAKE CALCULATION
                    bool noCreateCalculationError = await CreateFakeCalculation(launchId, modelId, parametersHash,
                                                                                characteristicsSet.Id, iterationIndex);                   
                    if (noCreateCalculationError)
                    {
                        // RETURN CHARACTERISTICS FROM DB
                        return CharacteristicValue.ListFromJsonElement(characteristicsSet.CharacteristicsValuesJson.RootElement);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            // CREATE REAL CALCULATION
            Calculation calculation = await CreateRealCalculation(launchId, modelId, parametersHash, iterationIndex);
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

        private async Task<Launch> CreateLaunch(string launchId, LaunchConfiguration launchConfiguration)
        {
            // method that creates launch and logs exceptions if they occur but does not propagate them
            Launch launch = new()
            {
                Id = launchId,
                Name = launchConfiguration.Name,
                Description = launchConfiguration.Description,
                UserId = launchConfiguration.User,
                UserParameters = JsonDocument.Parse(JsonSerializer.Serialize(launchConfiguration.UserParameters)),
                UserCharacteristics = JsonDocument.Parse(JsonSerializer.Serialize(launchConfiguration.Characteristics)),
                OptimizationAlgorithm = OptimizationAlgorithm.ToJsonDocument(launchConfiguration.OptimizationAlgorithm),
                RecalculateExisting = launchConfiguration.RecalculateExisting,
                DssSearchAccuracy = launchConfiguration.DssSearchAccuracy,
                Status = LaunchStatus.Running,
                StartTime = DateTime.UtcNow,
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
                                 ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(LogFilePath, message);
                noCreateLaunchError = false;
            }
            return (noCreateLaunchError) ? launch : null;
        }

        private async Task OnLaunchFinished(Launch launch, Task<ModelAndAlgorithmStatuses>[] modelsTasks)
        {
            // method that updates launch after end of all calculations and logs exceptions if they occur but does not propagate them
            launch.EndTime = DateTime.UtcNow;
            launch.Duration = launch.EndTime - launch.StartTime;
            launch.Status = Launch.GetLaunchStatus(modelsTasks);
            launch.OptimizationAlgorithm = OptimizationAlgorithm.UpdateModelsAlgorithmsStatuses(
                OptimizationAlgorithm.FromJsonElement(launch.OptimizationAlgorithm.RootElement),
                modelsTasks
            );
            
            try
            {
                await LaunchesRepository.Update(launch);
            }
            catch (Exception exception)
            {
                string LogFilePath = GetLaunchTmpFilePath(launch.Id, LogFileName);
                string message = "Unable to update launch due to database error!" + Environment.NewLine +
                                 ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(LogFilePath, message);
            }
        }

        private ModelStatus GetModelStatus(Task<List<CharacteristicValue>>[] calculationsTasks)
        {
            // this method calculates model status using statuses of calculations
            int totalTasks = 0;
            int completedTasks = 0;
            int failedTasks = 0;
            foreach (Task<List<CharacteristicValue>> task in calculationsTasks)
            {
                if (task != null)
                {
                    ++totalTasks;
                    if (task.Result == null)
                    {
                        ++failedTasks;
                    }
                    else
                    {
                        ++completedTasks;
                    }
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
            // this method runs preparer's process
            bool noPreparerError = true;
            try
            {
                string parametersJson = ParameterValue.ListToJsonString(parameters);
                CreateDirectoryIfNotExists(cadFormatParametersFilePath);
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
            // this method run's model process
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
                    CreateDirectoryIfNotExists(modelFormatCharacteristicsFilePath);
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
            // this method runs collector's process
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

        private async Task<string> CreateParametersSetIfNotExists(List<ParameterValue> parameters, string modelId, string launchId)
        {
            string parametersHash = ParameterValue.GetHashCode(parameters, modelId);

            // check if parameters set already exists
            ParametersSet? parametersSet = null;
            bool noGetParametersSetError = true;
            try
            {
                parametersSet = await ParametersRepository.GetById(parametersHash);
            }
            catch (Exception exception)
            {
                noGetParametersSetError = false;
                string logFilePath = GetModelTmpFilePath(launchId, modelId, LogFileName);
                string message = "Unable to get parameters set due to database exception!" + Environment.NewLine
                                 + ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }
            // got error while getting parameters
            if (!noGetParametersSetError)
            {
                return null;
            }
            // parameters set already exists
            if (parametersSet != null)
            {
                return parametersHash;
            }

            // create parameters set
            parametersSet = new()
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
                                 + ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }

            return (noCreateParametersSetError) ? parametersHash : null;
        }

        private CharacteristicsSet? FindCharacteristics(string launchId, string modelId, string parametersHash)
        {
            // this method tries to find newest characteristics for specified parameters set
            CharacteristicsSet? characteristicsSet = null;
            try
            {
                characteristicsSet = CalculationsRepository.Get()
                                         .Where(c => c.ParametersHash == parametersHash && c.Status == CalculationStatus.Completed)
                                         .OrderByDescending(c => c.EndTime)
                                         .Include(c => c.CharacteristicsSet)                                         
                                         .Select(c => c.CharacteristicsSet)
                                         .FirstOrDefault();
            }
            catch (Exception exception)
            {
                string logFilePath = GetModelTmpFilePath(launchId, modelId, LogFileName);
                string message = "Unable to find characteristics set due to database exception!" + Environment.NewLine
                                 + ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }
            return characteristicsSet;
        }

        private async Task<bool> CreateFakeCalculation(string launchId, string modelId,
                                                       string parametersHash, string characteristicsId,
                                                       int iterationIndex)
        {
            // this method is used for creating calculation if characteristics were found in db and thus no real calculations are needed
            Calculation calculation = new()
            {
                Id = Guid.NewGuid().ToString(),
                LaunchId = launchId,
                ModelId = modelId,
                ParametersHash = parametersHash,
                CharacteristicsId = characteristicsId,
                ReallyCalculated = false,
                IterationIndex = iterationIndex,
                Status = CalculationStatus.Completed,
                StartTime = DateTime.UtcNow,
                Message = null
            };
            calculation.EndTime = calculation.StartTime;
            calculation.Duration = calculation.EndTime - calculation.StartTime;

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
                                 + ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }
            return noCreateCalculationError;
        }

        private async Task<Calculation> CreateRealCalculation(string launchId, string modelId,
                                                              string parametersHash, int iterationIndex)
        {
            // this method is used for creating calculation if characteristics can not or should not be found in db and real calculations are needed
            Calculation calculation = new()
            {
                Id = Guid.NewGuid().ToString(),
                LaunchId = launchId,
                ModelId = modelId,
                ParametersHash = parametersHash,
                CharacteristicsId = null,
                ReallyCalculated = true,
                IterationIndex = iterationIndex,
                Status = CalculationStatus.Running,
                StartTime = DateTime.UtcNow,
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
                                 + ExceptionUtils.GetExceptionMessage(exception) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }

            return (noCreateCalculationError) ? calculation : null;
        }

        private static Process ConfigureProcess(string executableFilePath, string argumentsFormatString, string[] arguments)
        {
            // this methods helps to configure processes of executable files of different types
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
            // this method handles adding information about calculation exceptions to database or log files
            calculation.Message = messagePrefix + Environment.NewLine + ExceptionUtils.GetExceptionMessage(exception);
            calculation.EndTime = DateTime.UtcNow;
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
                                 ExceptionUtils.GetExceptionMessage(databaseException) + Environment.NewLine + Environment.NewLine;
                SafeAppendToFile(logFilePath, message);
            }
        }

        private async Task<List<CharacteristicValue>> ReadAndCreateCharacteristics(string characteristicsId,
                                                                                   string cadFormatCharacteristicsFilePath)
        {
            // this method reads characteristics from file created by collector and creates characteristics object in db
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
            // this method is used for updating info about calculation in db after calculation completion
            calculation.CharacteristicsId = characteristicsId;
            calculation.EndTime = DateTime.UtcNow;
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

        private void CreateDirectoryIfNotExists(string filePath)
        {
            string fileDirectory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }
        }

        private void SafeAppendToFile(string filePath, string message)
        {
            try
            {
                CreateDirectoryIfNotExists(filePath);
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
