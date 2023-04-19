using UhlnocsServer.Calculations;

namespace DSS
{
    public interface IConfigurationEnhancer
    {
        public Task<LaunchConfiguration> GetModifiedLaunchConfiguration(LaunchConfiguration launchConfiguration);
    }
}
