﻿using Grpc.Core;
using System.Text.Json;
using UhlnocsServer.Calculations;
using UhlnocsServer.Optimizations;
using UhlnocsServer.Users;
using static UhlnocsServer.Utils.ExceptionUtils;

namespace UhlnocsServer.Services
{
    public class CalculationService : CalculationServiceProto.CalculationServiceProtoBase
    {
        private readonly ILogger<CalculationService> Logger;
        private readonly UserService UserService;
        private CalculationsOptimizer Optimizer;

        public CalculationService(ILogger<CalculationService> logger,
                                  UserService userService,
                                  CalculationsOptimizer optimizer)
        {
            Logger = logger;
            UserService = userService;
            Optimizer = optimizer;
        }

        public override async Task<LaunchConfigurationMessage> StartLaunch(LaunchConfigurationMessage request, ServerCallContext context)
        {
            User sender = await UserService.AuthenticateUser(context);

            LaunchConfiguration configuration = null;
            try
            {
                JsonDocument configurationJsonDocument = JsonDocument.Parse(request.LaunchConfigurationJson);
                configuration = LaunchConfiguration.FromJsonDocument(configurationJsonDocument);
            }
            catch (Exception exception)
            {
                ThrowBadRequestException(exception);
            }

            _ = Optimizer.OptimizeLaunch(configuration);

            string configJsonString = LaunchConfiguration.ToJsonString(configuration);

            return new LaunchConfigurationMessage
            {
                LaunchConfigurationJson = configJsonString,
            };
        }
    }
}
