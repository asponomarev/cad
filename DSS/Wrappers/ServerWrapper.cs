using UhlnocsServer.Calculations;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Services;

namespace DSS.Wrappers
{
    public class ServerWrapper : IServerWrapper
    {
        public IList<LaunchConfiguration> GetLaunchConfigurations()
        {
            throw new NotImplementedException();
        }

        public ModelConfiguration GetModelConfiguration(string modelId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, CharacteristicWithModels> GetCharacteristicsWithModels()
        {
            return ModelService.CharacteristicsWithModels;
        }
    }
}
