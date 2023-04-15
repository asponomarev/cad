using UhlnocsServer.Calculations;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;

namespace DSS.Wrappers
{
    public interface IServerWrapper
    {
        public IList<LaunchConfiguration> GetLaunchConfigurations();

        public ModelConfiguration GetModelConfiguration(string modelId);

        public Dictionary<string, CharacteristicWithModels> GetCharacteristicsWithModels();
    }
}
