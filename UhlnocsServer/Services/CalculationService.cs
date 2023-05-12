using DSS;
using Grpc.Core;
using System.Data.Entity;
using System.Text.Json;
using UhlnocsServer.Calculations;
using UhlnocsServer.Calculations.LaunchesInfos;
using UhlnocsServer.Calculations.LaunchResult;
using UhlnocsServer.Database;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Optimizations;
using UhlnocsServer.Users;
using UhlnocsServer.Utils;

namespace UhlnocsServer.Services
{
    public class CalculationService : CalculationServiceProto.CalculationServiceProtoBase
    {
        private readonly ILogger<CalculationService> Logger;
        private readonly UserService UserService;
        private CalculationsOptimizer Optimizer;
        private IConfigurationEnhancer ConfigurationEnhancer;

        private readonly IRepository<Launch> LaunchesRepository;

        public CalculationService(ILogger<CalculationService> logger,
                                  UserService userService,
                                  CalculationsOptimizer optimizer,
                                  IConfigurationEnhancer configurationEnhancer,
                                  IRepository<Launch> launchesRepository)
        {
            Logger = logger;
            UserService = userService;
            Optimizer = optimizer;
            ConfigurationEnhancer = configurationEnhancer;
            LaunchesRepository = launchesRepository;
        }

        public override async Task<CalculationEmptyMessage> RecalculateModelPerformance(ModelIdMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            // we start performance recalculation task but don't wait for it because it can take a lot of time in theory
            _ = Optimizer.RecalculateModelPerformance(request.ModelId);

            return new CalculationEmptyMessage
            {

            };
        }

        
        public override async Task<LaunchConfigurationMessage> EnhanceLaunchConfiguration(LaunchConfigurationMessage request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            // get launch configuration from request
            LaunchConfiguration configuration = null;
            try
            {
                JsonDocument configurationJsonDocument = JsonDocument.Parse(request.LaunchConfigurationJson);
                configuration = LaunchConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception)
            {
                // bad request because we get exception here only if something is wrong with configuration in request
                ExceptionUtils.ThrowBadRequestException(exception);
            }

            // check permissions
            if (UserService.IsNotAdmin(sender.Id) && sender.Id != configuration.User)
            {
                string message = $"User {sender.Id} is not allowed to enhance launch as user {configuration.User}";
                throw new RpcException(new Status(StatusCode.PermissionDenied, message));
            }

            // wait for enhanced configuration and return it
            LaunchConfiguration enhancedConfiguration = await ConfigurationEnhancer.GetModifiedLaunchConfiguration(configuration);

            string configJsonString = LaunchConfiguration.ToJsonString(enhancedConfiguration);

            return new LaunchConfigurationMessage
            {
                LaunchConfigurationJson = configJsonString,
            };
        }

        public override async Task<LaunchIdMessage> StartLaunch(LaunchConfigurationMessage request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            // get launch configuration from request
            LaunchConfiguration configuration = null;
            try
            {
                JsonDocument configurationJsonDocument = JsonDocument.Parse(request.LaunchConfigurationJson);
                configuration = LaunchConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception)
            {
                // bad request because we get exception here only if something is wrong with configuration in request
                ExceptionUtils.ThrowBadRequestException(exception);
            }

            // check permissions
            if (UserService.IsNotAdmin(sender.Id) && sender.Id != configuration.User)
            {
                string message = $"User {sender.Id} is not allowed to start launch as user {configuration.User}";
                throw new RpcException(new Status(StatusCode.PermissionDenied, message));
            }

            // start launch task but don't wait for it because it can take a lot of time
            string launchId = Guid.NewGuid().ToString();
            _ = Optimizer.OptimizeLaunch(launchId, configuration);

            return new LaunchIdMessage
            {
                LaunchId = launchId
            };
        }

        public override async Task<LaunchesInfosReply> GetLaunchesInfos(LaunchesFiltersRequest request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            LaunchesFilters launchesFilters = LaunchesFilters.FromJsonString(request.LaunchesFiltersJson);
            var launchesQuery = LaunchesRepository.Get();  // query is NOT executed here

            // add where clauses to query if corresponding filters are set
            if (launchesFilters.LaunchNamePart != null)
            {
                launchesQuery = launchesQuery.Where(l => l.Name.Contains(launchesFilters.LaunchNamePart));
            }
            if (launchesFilters.UserIdPart != null)
            {
                launchesQuery = launchesQuery.Where(l => l.UserId.Contains(launchesFilters.UserIdPart));
            }
            if (launchesFilters.Status != null)
            {
                launchesQuery = launchesQuery.Where(l => l.Status == launchesFilters.Status);
            }
            if (launchesFilters.StartTimeMin != null)
            {
                launchesQuery = launchesQuery.Where(l => l.StartTime >= launchesFilters.StartTimeMin);
            }
            if (launchesFilters.StartTimeMax != null)
            {
                launchesQuery = launchesQuery.Where(l => l.StartTime <= launchesFilters.StartTimeMax);
            }
            if (launchesFilters.EndTimeMin != null)
            {
                launchesQuery = launchesQuery.Where(l => l.EndTime != null && l.EndTime >= launchesFilters.EndTimeMin);
            }
            if (launchesFilters.EndTimeMax != null)
            {
                launchesQuery = launchesQuery.Where(l => l.EndTime != null && l.EndTime <= launchesFilters.EndTimeMax);
            }
            if (launchesFilters.DurationMin != null)
            {
                launchesQuery = launchesQuery.Where(l => l.Duration != null && l.Duration >= launchesFilters.DurationMin);
            }
            if (launchesFilters.DurationMax != null)
            {
                launchesQuery = launchesQuery.Where(l => l.Duration != null && l.Duration <= launchesFilters.DurationMax);
            }

            // execute count query (relatively fast query)
            int launchesTotal = launchesQuery.Count();

            // prepare for data pagination
            int pageSize = launchesFilters.PageSize;
            int page = launchesFilters.Page - 1;  // in request numbering starts from 1
            launchesQuery = launchesQuery.OrderByDescending(l => l.StartTime)
                                         .Skip(page * pageSize)
                                         .Take(pageSize);

            // calculate total amount of pages for reply
            int pagesTotal = launchesTotal / pageSize;  
            if (launchesTotal % pageSize != 0)
            {
                ++pagesTotal;
            }

            // complete and execute get data query
            List<ShortLaunchInfo> launchesInfos = launchesQuery.Select(l => new ShortLaunchInfo(l.Id, l.Name, l.UserId, l.Status,
                                                                                                l.StartTime, l.EndTime, l.Duration))
                                                               .ToList();
            return new LaunchesInfosReply
            {
                LaunchesTotal = launchesTotal,
                PagesTotal = pagesTotal,
                LaunchesInfosJson = ShortLaunchInfo.ListToJsonString(launchesInfos)
            };
        }

        public override async Task<LaunchResultReply> GetLaunchResult(LaunchIdMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            // get launch and load all dependent entities
            Launch? launch = null;
            try
            {
                // combination of Include and ThenInclude does not work
                // usage of Select or Join works but looks worse than this
                // feel free to improve this code fragment
                // things we want to do here are too specific to add methods to repository so we just create context
                using (ApplicationDatabaseContext dbContext = new())  
                {
                    launch = dbContext.Launches.FirstOrDefault(l => l.Id == request.LaunchId);
                    if (launch != null)
                    {
                        dbContext.Entry(launch).Collection(l => l.Calculations).Load();  // using collection navigation
                        foreach (Calculation calculation in launch.Calculations)
                        {
                            // using single reference navigation
                            dbContext.Entry(calculation).Reference(c => c.Model).Load();
                            dbContext.Entry(calculation).Reference(c => c.ParametersSet).Load();
                            dbContext.Entry(calculation).Reference(c => c.CharacteristicsSet).Load();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowUnknownException(exception);
            }
            if (launch == null)
            {
                string exceptionMessage = $"Launch with id '{request.LaunchId}' not found";
                throw new RpcException(new Status(StatusCode.NotFound, exceptionMessage));
            }

            // create full launch info object
            List<string> parametersSetByUser = JsonSerializer.Deserialize<List<string>>(launch.UserParameters);
            List<CharacteristicWithModel> characteristicsWantedByUser = JsonSerializer.Deserialize<List<CharacteristicWithModel>>(launch.UserCharacteristics);
            FullLaunchInfo fullLaunchInfo = new(launch.Id,
                                                launch.Name,
                                                launch.Description,
                                                launch.UserId,
                                                launch.RecalculateExisting,
                                                launch.DssSearchAccuracy,
                                                launch.Status,
                                                launch.StartTime,
                                                launch.EndTime,
                                                launch.Duration,
                                                parametersSetByUser,
                                                characteristicsWantedByUser);

            // create optimization algorithm info object
            OptimizationAlgorithm algorithm = OptimizationAlgorithm.FromJsonElement(launch.OptimizationAlgorithm.RootElement);
            OptimizationAlgorithmInfo optimizationAlgorithmInfo = OptimizationAlgorithm.ToInfo(algorithm);

            // first two of these are dictionaries for comfort of use in this method
            Dictionary<string, ModelInfo> modelsInfos = new();
            Dictionary<string, ParameterResult> parametersResults = new();
            List<CharacteristicResult> characteristicsResults = new();

            int iterationsAmount = launch.Calculations.Max(c => c.IterationIndex) + 1;  // +1 because iterations are numbered from 0
            List<Calculation> sortedCalculations = launch.Calculations.OrderBy(c => c.ModelId).ThenBy(c => c.IterationIndex).ToList();

            foreach (Calculation calculation in sortedCalculations)
            {
                // create model info if not exists
                string modelId = calculation.ModelId;
                if (!modelsInfos.ContainsKey(modelId))
                {
                    string modelName = ModelConfiguration.FromJsonDocument(calculation.Model.Configuration).Name;
                    AlgorithmStatus modelAlgorithmStatus = algorithm.ModelsAlgorithmsStatuses.GetValueOrDefault(modelId, AlgorithmStatus.Undefined);
                    List<CalculationInfo> modelCalculationsInfos = new();
                    ModelInfo modelInfo = new(modelId, modelName, modelAlgorithmStatus, modelCalculationsInfos);
                    modelsInfos.Add(modelId, modelInfo);
                }

                // create calculation info and add it to model calculations info list
                int iterationIndex = calculation.IterationIndex;
                CalculationInfo calculationInfo = new(calculation.Id,
                                                      calculation.Status,
                                                      calculation.ReallyCalculated,
                                                      iterationIndex,
                                                      calculation.StartTime,
                                                      calculation.EndTime,
                                                      calculation.Duration,
                                                      calculation.Message);
                modelsInfos[modelId].CalculationsInfos.Add(calculationInfo);
               
                // fill in parameters results
                List<ParameterValue> parameters = ParameterValue.ListFromJsonDocument(calculation.ParametersSet.ParametersValuesJson);
                foreach (ParameterValue parameter in parameters)
                {
                    // create parameter result if not exists
                    string parameterId = parameter.Id;

                    if (!parametersResults.ContainsKey(parameterId))
                    {
                        List<string> usedByModels = new();
                        List<object?> parameterValues = new();
                        for (int i = 0; i < iterationsAmount; ++i)
                        {
                            parameterValues.Add(null);
                        }
                        ParameterResult parameterResult = new(parameterId, usedByModels, parameterValues);
                        parametersResults.Add(parameterId, parameterResult);
                    }

                    // add model to list of models which use this parameter
                    if (!parametersResults[parameterId].UsedByModels.Contains(modelId))
                    {
                        parametersResults[parameterId].UsedByModels.Add(modelId);
                    }

                    // add parameter value at iteration to list of parameter values
                    if (parametersResults[parameterId].Values[iterationIndex] == null)
                    {
                        parametersResults[parameterId].Values[iterationIndex] = ParameterValue.GetValue(parameter);
                    }                   
                }

                // fill in characteristics results
                List<CharacteristicValue> characteristics = CharacteristicValue.ListFromJsonElement(calculation.CharacteristicsSet.CharacteristicsValuesJson.RootElement);
                foreach (CharacteristicValue characteristic in characteristics)
                {
                    string characteristicId = characteristic.Id;

                    bool characteristicNotFound = true;
                    int characteristicIndexInResult = 0;
                    while (characteristicIndexInResult < characteristicsResults.Count && characteristicNotFound)
                    {
                        if (characteristicsResults[characteristicIndexInResult].Id == characteristicId &&
                            characteristicsResults[characteristicIndexInResult].CalculatedByModel == modelId)
                        {
                            characteristicNotFound = false;
                        }
                        else
                        {
                            ++characteristicIndexInResult;
                        }
                    }

                    // create characteristic result if not exists
                    if (characteristicNotFound)
                    {
                        List<object?> characteristicValues = new();
                        for (int i = 0; i < iterationsAmount; ++i)
                        {
                            characteristicValues.Add(null);
                        }
                        CharacteristicResult characteristicResult = new(characteristicId, modelId, characteristicValues);
                        characteristicsResults.Add(characteristicResult);
                    }

                    // add characteristic value at iteration to list of characteristic values
                    if (characteristicsResults[characteristicIndexInResult].Values[iterationIndex] == null)
                    {
                        characteristicsResults[characteristicIndexInResult].Values[iterationIndex] = CharacteristicValue.GetValue(characteristic);
                    }
                }
            }

            // create launch result object
            List<ModelInfo> sortedModelsInfos = modelsInfos.Values.OrderBy(mi => mi.ModelId).ToList();
            List<ParameterResult> sortedParametersResults = parametersResults.Values.OrderBy(pr => pr.Id).ToList();
            List<CharacteristicResult> sortedCharacteristicResults = characteristicsResults.OrderBy(cr => cr.Id).ThenBy(cr => cr.CalculatedByModel).ToList();
            LaunchResult launchResult = new(fullLaunchInfo,
                                            optimizationAlgorithmInfo,
                                            sortedModelsInfos,
                                            sortedParametersResults,
                                            sortedCharacteristicResults);

            string launchResultJson = LaunchResult.ToJsonString(launchResult);
            return new LaunchResultReply 
            {
                LaunchResultJson = launchResultJson
            };
        }
    }
}
