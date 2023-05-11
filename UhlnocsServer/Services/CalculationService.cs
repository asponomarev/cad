using DSS;
using Grpc.Core;
using System.Data.Entity;
using System.Text.Json;
using UhlnocsServer.Calculations;
using UhlnocsServer.Calculations.LaunchesInfos;
using UhlnocsServer.Calculations.LaunchResult;
using UhlnocsServer.Database;
using UhlnocsServer.Optimizations;
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
            await UserService.AuthenticateUser(context);  // todo: add check that user is responsible for model

            _ = Optimizer.RecalculateModelPerformance(request.ModelId);

            return new CalculationEmptyMessage
            {

            };
        }

        
        public override async Task<LaunchConfigurationMessage> EnhanceLaunchConfiguration(LaunchConfigurationMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);  // todo: add check that user is responsible for launch

            LaunchConfiguration configuration = null;
            try
            {
                JsonDocument configurationJsonDocument = JsonDocument.Parse(request.LaunchConfigurationJson);
                configuration = LaunchConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowBadRequestException(exception);
            }

            LaunchConfiguration enhancedConfiguration = await ConfigurationEnhancer.GetModifiedLaunchConfiguration(configuration);

            string configJsonString = LaunchConfiguration.ToJsonString(enhancedConfiguration);

            return new LaunchConfigurationMessage
            {
                LaunchConfigurationJson = configJsonString,
            };
        }

        public override async Task<LaunchIdMessage> StartLaunch(LaunchConfigurationMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);  // todo: add check that user is responsible for launch

            LaunchConfiguration configuration = null;
            try
            {
                JsonDocument configurationJsonDocument = JsonDocument.Parse(request.LaunchConfigurationJson);
                configuration = LaunchConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception)
            {
                ExceptionUtils.ThrowBadRequestException(exception);
            }

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

        public override async Task<LaunchResultReply> GetlaunchResult(LaunchIdMessage request, ServerCallContext context)
        {
            await UserService.AuthenticateUser(context);

            Launch? launch = null;
            try
            {
                // get launch and include all dependent entities
                launch = LaunchesRepository.Get()
                             .Where(l => l.Id == request.LaunchId)
                             .Include(l => l.Calculations.Select(c => c.Model))
                             .Include(l => l.Calculations.Select(c => c.ParametersSet))
                             .Include(l => l.Calculations.Select(c => c.CharacteristicsSet))
                             .FirstOrDefault();
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

            FullLaunchInfo fullLaunchInfo = new(launch.Id, launch.Name, launch.Description, launch.UserId, launch.RecalculateExisting,
                                                launch.DssSearchAccuracy, launch.Status, launch.StartTime, launch.EndTime, launch.Duration);

            OptimizationAlgorithm algorithm = OptimizationAlgorithm.FromJsonElement(launch.OptimizationAlgorithm.RootElement);
            OptimizationAlgorithmInfo optimizationAlgorithmInfo = OptimizationAlgorithm.ToInfo(algorithm);

            
        }
    }
}
