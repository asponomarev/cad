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

            bool noCalculationError = true; // ошибка расчета характеристик
            List<ParameterValue> calculationParameters = new(); // параметры модели для проведения расчетов
            string variableParameterId = modelLaunchConfiguration.OptimizationAlgorithm.VariableParameter;
            ParameterValue variableParameter = modelLaunchConfiguration.GetParameterValue(variableParameterId);
            PropertyValueType valueType = ModelService.ParametersWithModels[variableParameterId].ValueType;

            /* Constant Step Search */
            Task[] calculationsTasks = null;
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

            /* Smart Constant Step Search */
            else if (modelLaunchConfiguration.OptimizationAlgorithm is SmartConstantStep smartConstantStep)
            {
                int iteration = 0;
                double firstValue = (variableParameter as DoubleParameterValue).Value;
                double variableParameterValue = 0;
                double throughputCharacteristicValue = 0;
                do
                {
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
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
                        throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, smartConstantStep.ThroughputCharacteristic);
                    }

                    ++iteration;
                }
                while (noCalculationError &&
                       iteration < smartConstantStep.MaxIterations &&
                       OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, smartConstantStep.Accuracy));
            }

            /* Binary Search */
            else if (modelLaunchConfiguration.OptimizationAlgorithm is BinarySearch binarySearch)
            {
                binarySearch.FirstValue = (variableParameter as DoubleParameterValue).Value;
                binarySearch.LastValue = binarySearch.MaxRate;

                // first iter check

                for (int i = 1; i < binarySearch.Iterations; ++i)
                {
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
                            double variableParameterValue = BinarySearch.MakeParameterValue(binarySearch.FirstValue, binarySearch.LastValue);
                            calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                        }
                    }
                    List<CharacteristicValue> calculationCharacteristics = await OptimizeCalculation(launchId, modelConfiguration, calculationParameters);
                    if (calculationCharacteristics == null)
                    {
                        noCalculationError = false;
                        break;
                    }
                    else
                    {
                        throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, binarySearch.ThroughputCharacteristic);
                        if (OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, binarySearch.Accuracy))
                        {
                            binarySearch.FirstValue = variableParameterValue;
                        }
                        else
                        {
                            binarySearch.LastValue = variableParameterValue;
                        }
                    }
                }
            }

            /* Smart Binary Search */
            else if (modelLaunchConfiguration.OptimizationAlgorithm is SmartBinarySearch smartBinarySearch)
            {
                int iteration = 0;
                double firstValue = (variableParameter as DoubleParameterValue).Value;
                double lastValue = smartBinarySearch.MaxRate;
                double variableParameterValue = 0;
                string firstChangedBorder = null;
                bool bothBordersChanged = false;
                do
                {
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
                            variableParameterValue = (firstValue + lastValue) / 2;
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
                        throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, smartBinarySearch.ThroughputCharacteristic);
                        if (OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, binarySearch.Accuracy))
                        {
                            firstValue = variableParameterValue;
                            if (firstChangedBorder == null)
                            {
                                firstChangedBorder = "Left";
                            }
                            else if (firstChangedBorder == "Right")
                            {
                                bothBordersChanged = true;
                            }
                            else if (firstChangedBorder == "Left" && bothBordersChanged == true)
                            {
                                break;
                            }
                        }
                        else
                        {
                            lastValue = variableParameterValue;
                            if (firstChangedBorder == null)
                            {
                                firstChangedBorder = "Right";
                            }
                            else if (firstChangedBorder == "Left")
                            {
                                bothBordersChanged = true;
                            }
                            else if (firstChangedBorder == "Right" && bothBordersChanged == true)
                            {
                                break;
                            }
                        }
                    }
                    ++iteration;
                }
                while (noCalculationError && iteration < smartBinarySearch.MaxIterations);
            }

            /* Golden Section Search */
            else if (modelLaunchConfiguration.OptimizationAlgorithm is GoldenSection goldenSection)
            {
                const double PHI_number = 1.618; // Fibonacci number
                double firstValue = (variableParameter as DoubleParameterValue).Value;
                double lastValue = goldenSection.MaxRate;
                double variableParameterValue = 0;
                double X1;
                double X2;
                string lastFoundPoint;
                string nextPoint = "X1";
                for (int i = 0; i < goldenSection.Iterations; ++i)
                {
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
                            if (nextPoint == "X1")
                            {
                                X1 = lastValue - (lastValue - firstValue) / PHI_number;
                                variableParameterValue = X1;
                                lastFoundPoint == "X1";
                                nextPoint = "X2";
                            }
                            else // nextPoint = X2
                            {
                                X2 = firstValue + (lastValue - firstValue) / PHI_number;
                                variableParameterValue = X2;
                                lastFoundPoint = "X2";
                                nextPoint = "X1";
                            }
                            calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                        }
                    }
                    List<CharacteristicValue> calculationCharacteristics = await OptimizeCalculation(launchId, modelConfiguration, calculationParameters);
                    if (calculationCharacteristics == null)
                    {
                        noCalculationError = false;
                        break;
                    }
                    else
                    {
                        throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, goldenSection.ThroughputCharacteristic);
                        if (lastFoundPoint == "X1")
                        {
                            if (OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, goldenSection.Accuracy) == false)
                            {
                                lastValue = X1;
                                nextPoint = "X1";
                            }
                        }
                        else // lastFoundPoint == X2
                        {
                            if (OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, goldenSection.Accuracy))
                            {
                                firstValue = X2;
                            }
                            else // X2 - bad
                            {
                                firstValue = X1;
                                lastValue = X2;
                            }
                        }
                    }
                }
            }

            /* Smart Golden Section Search */
            else if (modelLaunchConfiguration.OptimizationAlgorithm is SmartGoldenSection smartGoldenSection)
            {
                const double PHI_number = 1.618; // Fibonacci number
                double firstValue = (variableParameter as DoubleParameterValue).Value;
                double lastValue = smartGoldenSection.MaxRate;
                double variableParameterValue = 0;
                double X1;
                double X2;
                string lastFoundPoint;
                string nextPoint = "X1";
                bool inMiddleSegment = false;
                do
                {
                    foreach (ParameterValue parameter in modelLaunchConfiguration.Parameters)
                    {
                        if (parameter.Id != variableParameterId)
                        {
                            calculationParameters.Add(parameter);  // this may be bad
                        }
                        else
                        {
                            if (nextPoint == "X1")
                            {
                                X1 = lastValue - (lastValue - firstValue) / PHI_number;
                                variableParameterValue = X1;
                                lastFoundPoint == "X1";
                                nextPoint = "X2";
                            }
                            else // nextPoint = X2
                            {
                                X2 = firstValue + (lastValue - firstValue) / PHI_number;
                                variableParameterValue = X2;
                                lastFoundPoint = "X2";
                                nextPoint = "X1";
                            }
                            calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                        }
                    }
                    List<CharacteristicValue> calculationCharacteristics = await OptimizeCalculation(launchId, modelConfiguration, calculationParameters);
                    if (calculationCharacteristics == null)
                    {
                        noCalculationError = false;
                        break;
                    }
                    else
                    {
                        throughputCharacteristicValue = CharacteristicValue.GetThroughputValue(calculationCharacteristics, smartGoldenSection.ThroughputCharacteristic);
                        if (lastFoundPoint == "X1")
                        {
                            if (OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, goldenSection.Accuracy) == false)
                            {
                                lastValue = X1;
                                nextPoint = "X1";
                                if (inMiddleSegment == true)
                                {
                                    break;
                                }
                            }
                        }
                        else // lastFoundPoint == X2
                        {
                            if (OptimizationAlgorithm.IsPointGood(variableParameterValue, throughputCharacteristicValue, goldenSection.Accuracy))
                            {
                                firstValue = X2;
                                if (inMiddleSegment == true)
                                {
                                    break;
                                }
                            }
                            else // X2 - bad
                            {
                                firstValue = X1;
                                lastValue = X2;
                                inMiddleSegment = true;
                            }
                        }
                    }
                }
                while (noCalculationError && iteration < smartGoldenSection.MaxIterations);
            }
            // c vТут будет какой-то код Саши
        }

        public async Task<List<CharacteristicValue>> OptimizeCalculation(string launchId,
                                                                         ModelConfiguration modelConfiguration,
                                                                         List<ParameterValue> parameters)
        {          
            string modelId = modelConfiguration.Id;

            // CREATE PARAMETERS
            string parametersHash = ParameterValue.GetHashCode(parameters, modelId);
            ParametersSet parametersSet = new ParametersSet
            {
                Hash = parametersHash,
                ParametersValuesJson = ParameterValue.ListToJsonDocument(parameters)
            };
            bool noCreateParametersSetError = true;
            try
            {
                await ParametersRepository.Create(parametersSet);
            }
            catch (Exception)
            {
                noCreateParametersSetError = false;
            }
            if (!noCreateParametersSetError)
            {
                // think about logging this
                return null;
            }

            // CREATE CALCULATION
            string calculationId = Guid.NewGuid().ToString();
            Calculation calculation = new()
            {
                Id = calculationId,
                LaunchId = launchId,
                ModelId = modelId,
                ParametersHash = parametersHash,
                CharacteristicsHash = null,
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
            catch (Exception)
            {
                noCreateCalculationError = false;
            }
            if (!noCreateCalculationError)
            {
                // think about logging this
                return null;
            }

            // PREPARE PARAMETERS
            string modelPreparerFilePath = GetModelFilePath(modelId, modelConfiguration.PreparerFilePath);
            string cadFormatParametersFilePath = GetTmpFilePath(launchId, modelId, calculationId, "params_in_cad_format");
            string modelFormatParametersFilePath = GetTmpFilePath(launchId, modelId, calculationId, "params_in_model_format");
            bool noPreparerError = await PrepareParameters(modelPreparerFilePath, modelConfiguration.PreparerOkExitCode,
                                                           cadFormatParametersFilePath, modelFormatParametersFilePath,
                                                           parameters, calculation);
            if (!noPreparerError) 
            {
                // should be already logged
                return null;
            }

            // RUN MODEL

            // COLLECT CHARACTERISTICS

            return new List<CharacteristicValue>(); // will be changed later
        }

        public async Task<bool> PrepareParameters(string modelPreparerFilePath,
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

                Process prepareProcess = new Process();
                prepareProcess.StartInfo.FileName = modelPreparerFilePath;
                prepareProcess.StartInfo.CreateNoWindow = true;
                prepareProcess.StartInfo.Arguments = $"--in_file_path {cadFormatParametersFilePath} --out_file_path {modelFormatParametersFilePath}";
                prepareProcess.Start();
                prepareProcess.WaitForExit();
                if (prepareProcess.ExitCode != preparerOkExitCode)
                {
                    throw new Exception($"Preparer process finished with exit code {prepareProcess.ExitCode}");
                }
            }
            catch (Exception exception)
            {
                calculation.Message = GetInternalExceptionMessage(exception);
                calculation.EndTime = DateTime.Now;
                calculation.Duration = calculation.EndTime - calculation.StartTime;
                calculation.Status = CalculationStatus.Failed;
                try
                {                
                    await CalculationsRepository.Update(calculation);
                } 
                catch (Exception)
                {
                    // think about logging this
                }
                noPreparerError = false;
            }
            return noPreparerError;
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
