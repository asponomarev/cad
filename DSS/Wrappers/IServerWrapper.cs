using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;

namespace DSS.Wrappers
{
    public interface IServerWrapper
    {
        public Task<List<List<ParameterValue>>> GetModelParametersValues(string modelId);

        public Task<ModelConfiguration> GetModelConfiguration(string modelId);

        public Dictionary<string, CharacteristicWithModels> GetCharacteristicsWithModels();
    }
}
