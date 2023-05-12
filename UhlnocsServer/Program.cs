using DSS;
using UhlnocsServer.Calculations;
using UhlnocsServer.Database;
using UhlnocsServer.Models;
using UhlnocsServer.Optimizations;
using UhlnocsServer.Services;
using UhlnocsServer.Users;

namespace UhlnocsServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();

            // configure services DI
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<ModelService>();
            builder.Services.AddSingleton<CalculationService>();
            builder.Services.AddSingleton<CalculationsOptimizer>();
            builder.Services.AddSingleton<IConfigurationEnhancer, ConfigurationEnhancer>();

            // configure repositories DI
            builder.Services.AddTransient<IRepository<User>, Repository<User>>();
            builder.Services.AddTransient<IRepository<Model>, Repository<Model>>();
            builder.Services.AddTransient<IRepository<Launch>, Repository<Launch>>();
            builder.Services.AddTransient<IRepository<Calculation>, Repository<Calculation>>();
            builder.Services.AddTransient<IRepository<ParametersSet>, Repository<ParametersSet>>();
            builder.Services.AddTransient<IRepository<CharacteristicsSet>, Repository<CharacteristicsSet>>();

            var app = builder.Build();

            // configure gRpc services
            app.MapGrpcService<UserService>();
            app.MapGrpcService<ModelService>();
            app.MapGrpcService<CalculationService>();

            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}