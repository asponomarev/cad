using DSS;
using Grpc.Core;
using System.Text.Json;
using UhlnocsServer.Calculations;
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

        public CalculationService(ILogger<CalculationService> logger,
                                  UserService userService,
                                  CalculationsOptimizer optimizer,
                                  IConfigurationEnhancer configurationEnhancer)
        {
            Logger = logger;
            UserService = userService;
            Optimizer = optimizer;
             ConfigurationEnhancer = configurationEnhancer;
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
    }
}
