namespace UhlnocsServer.Utils
{
    public static class PropertiesHolder
    {
        // this path will be changed to GetCurrentDirectory() + \\Properties\\ in release version
        public const string PropertiesDirectory = "C:\\Projects\\Diploma\\Uhlnocs\\UhlnocsServer\\Properties\\";

        public static readonly IConfiguration LaunchSettings = new ConfigurationBuilder()
                                                                   .SetBasePath(PropertiesDirectory)
                                                                   .AddJsonFile("launchSettings.json", false, true)
                                                                   .Build();

        public static readonly IConfiguration DatabaseSettings = new ConfigurationBuilder()
                                                                     .SetBasePath(PropertiesDirectory)
                                                                     .AddJsonFile("databaseSettings.json", false, true)
                                                                     .Build();

        public static readonly IConfiguration UserSettings = new ConfigurationBuilder()
                                                                 .SetBasePath(PropertiesDirectory)
                                                                 .AddJsonFile("userSettings.json", false, true)
                                                                 .Build();

        public static readonly IConfiguration ModelSettings = new ConfigurationBuilder()
                                                                  .SetBasePath(PropertiesDirectory)
                                                                  .AddJsonFile("modelSettings.json", false, true)
                                                                  .Build();
    }
}
