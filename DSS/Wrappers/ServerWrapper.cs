using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Optimizations;
using UhlnocsServer.Services;

namespace DSS.Wrappers
{
    public class ServerWrapper : IServerWrapper
    {
        private readonly ModelService _modelService;
        private readonly CalculationsOptimizer _calculationsOptimizer;

        public ServerWrapper(ModelService modelService, CalculationsOptimizer calculationsOptimizer) 
        {
            _modelService = modelService;
            _calculationsOptimizer = calculationsOptimizer;
        }

        public async Task<List<List<ParameterValue>>> GetModelParametersValues(string modelId)
        {
            return await _calculationsOptimizer.GetModelParametersValues(modelId);
        }

        public async Task<ModelConfiguration> GetModelConfiguration(string modelId)
        {
            return await _modelService.GetModelConfigurationInternal(modelId);
        }

        public Dictionary<string, CharacteristicWithModels> GetCharacteristicsWithModels()
        {
            return ModelService.CharacteristicsWithModels;
        }
    }
}
