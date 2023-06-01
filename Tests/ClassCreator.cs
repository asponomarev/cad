using UhlnocsServer.Calculations;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Optimizations;

namespace Tests
{
    public static class ClassCreator
    {
        public static IList<LaunchConfiguration> GetLaunchConfigurations()
        {
            var listParameters = GetListsParameters();
            return new List<LaunchConfiguration>()
            {
                new LaunchConfiguration(
                    "Example Configuration 1",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[0],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 2",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[1],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 3",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[2],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 4",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[3],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 5",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[4],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model5"),
                        new CharacteristicWithModel("Id2", "Model6"),
                        new CharacteristicWithModel("Id3", "Model7")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 6",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[5],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 7",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[6],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 8",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[7],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 9",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[8],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 10",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[9],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep( "", "", 0, 0, 0),
                    true,
                    0.9
                ),
                new LaunchConfiguration(
                    "Example Configuration 11",
                    "This is an example configuration",
                    "John Smith",
                    listParameters[10],
                    new List<string>() { "UserParam1", "UserParam2", "UserParam3" },
                    new List<CharacteristicWithModel>()
                    {
                        new CharacteristicWithModel("Id1", "Model1"),
                        new CharacteristicWithModel("Id2", "Model2"),
                        new CharacteristicWithModel("Id3", "Model3")
                    },
                    new SmartConstantStep("", "", 0, 0, 0),
                    true,
                    0.9
                )
            };
        }

        public static List<List<ParameterValue>> GetListsParameters()
        {
            return new List<List<ParameterValue>>()
            {
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 1),
                    new IntParameterValue("Parameter2", 2),
                    new IntParameterValue("Parameter3", 3),
                    new DoubleParameterValue("Parameter4", 4.4),
                    new DoubleParameterValue("Parameter5", 5.5),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 5),
                    new IntParameterValue("Parameter2", 6),
                    new IntParameterValue("Parameter3", 7),
                    new DoubleParameterValue("Parameter4", 8.8),
                    new DoubleParameterValue("Parameter5", 10.10),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 3),
                    new IntParameterValue("Parameter2", 4),
                    new IntParameterValue("Parameter3", 5),
                    new DoubleParameterValue("Parameter4", 6.6),
                    new DoubleParameterValue("Parameter5", 7.7),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 1),
                    new IntParameterValue("Parameter2", 1),
                    new IntParameterValue("Parameter3", 1),
                    new DoubleParameterValue("Parameter4", 1.1),
                    new DoubleParameterValue("Parameter5", 1.1),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 3),
                    new IntParameterValue("Parameter2", 5),
                    new IntParameterValue("Parameter3", 7),
                    new DoubleParameterValue("Parameter4", 1.1),
                    new DoubleParameterValue("Parameter5", 3.3),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 5),
                    new IntParameterValue("Parameter2", 4),
                    new IntParameterValue("Parameter3", 3),
                    new DoubleParameterValue("Parameter4", 2.2),
                    new DoubleParameterValue("Parameter5", 1.1),
                    new BoolParameterValue("Parameter6", false),
                    new BoolParameterValue("Parameter7", true),
                    new StringParameterValue("Parameter8", "Parameter81")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 8),
                    new IntParameterValue("Parameter2", 8),
                    new IntParameterValue("Parameter3", 2),
                    new DoubleParameterValue("Parameter4", 1.1),
                    new DoubleParameterValue("Parameter5", 7.7),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", true),
                    new StringParameterValue("Parameter8", "Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 9),
                    new IntParameterValue("Parameter2", 5),
                    new IntParameterValue("Parameter3", 1),
                    new DoubleParameterValue("Parameter4", 11.11),
                    new DoubleParameterValue("Parameter5", 5.5),
                    new BoolParameterValue("Parameter6", false),
                    new BoolParameterValue("Parameter7", true),
                    new StringParameterValue("Parameter8", "Parameter8")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 9),
                    new IntParameterValue("Parameter2", 9),
                    new IntParameterValue("Parameter3", 9),
                    new DoubleParameterValue("Parameter4", 9.9),
                    new DoubleParameterValue("Parameter5", 9.9),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "123456")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 50),
                    new IntParameterValue("Parameter2", 20),
                    new IntParameterValue("Parameter3", 30),
                    new DoubleParameterValue("Parameter4", 40.4),
                    new DoubleParameterValue("Parameter5", 50.5),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter80")
                },
                new List<ParameterValue>()
                {
                    new IntParameterValue("Parameter1", 3),
                    new IntParameterValue("Parameter2", 3),
                    new IntParameterValue("Parameter3", 3),
                    new DoubleParameterValue("Parameter4", 3.3),
                    new DoubleParameterValue("Parameter5", 3.3),
                    new BoolParameterValue("Parameter6", true),
                    new BoolParameterValue("Parameter7", false),
                    new StringParameterValue("Parameter8", "Parameter8Parameter8")
                },
            };
        }

        public static Dictionary<string, CharacteristicWithModels> GetCharacteristics()
        {
            var dict = new Dictionary<string, CharacteristicWithModels>
            {
                { "Id1", new CharacteristicWithModels("Id1", "Model1", PropertyValueType.Int, new List<string> { "Model1" }) },
                { "Id2", new CharacteristicWithModels("Id2", "Model2", PropertyValueType.Bool, new List<string> { "Model2" }) },
                { "Id3", new CharacteristicWithModels("Id3", "Model3", PropertyValueType.Double, new List<string> { "Model3" }) },
                { "Id4", new CharacteristicWithModels("Id4", "Model4", PropertyValueType.String, new List<string> { "Model4" }) },

                { "Id5", new CharacteristicWithModels("Id5", "Model1", PropertyValueType.Int, new List<string> { "Model1" }) },
            };
            return dict;
        }
    }
}
