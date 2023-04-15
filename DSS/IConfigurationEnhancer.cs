using UhlnocsServer.Calculations;

namespace DSS
{
    public interface IConfigurationEnhancer
    {
        public LaunchConfiguration GetModifiedLaunchConfiguration(LaunchConfiguration launchConfiguration);
    }
}
