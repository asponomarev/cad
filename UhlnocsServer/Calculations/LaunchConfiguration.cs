using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Optimizations;

namespace UhlnocsServer.Calculations
{
    // this class contains info about launch and is also used in messages sent via gRpc
    public class LaunchConfiguration
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string User { get; set; }

        public List<ParameterValue> Parameters { get; set; }

        public List<string> UserParameters { get; set; }

        public List<CharacteristicWithModel> Characteristics { get; set; }

        public OptimizationAlgorithm OptimizationAlgorithm { get; set; }

        public bool RecalculateExisting { get; set; }

        public double DssSearchAccuracy { get; set; }

        public LaunchConfiguration(string name,
                                   string description,
                                   string user,
                                   List<ParameterValue> parameters,
                                   List<string> userParameters,
                                   List<CharacteristicWithModel> characteristics,
                                   OptimizationAlgorithm algorithm,
                                   bool recalculateExisting,
                                   double dssSearchAccuracy)
        {
            Name = name;
            Description = description;
            User = user;
            Parameters = parameters;
            UserParameters = userParameters;
            Characteristics = characteristics;
            OptimizationAlgorithm = algorithm;
            RecalculateExisting = recalculateExisting;
            DssSearchAccuracy = dssSearchAccuracy;
        }

        public static LaunchConfiguration FromJsonDocument(JsonDocument configJsonDocument)
        {
            JsonElement configRoot = configJsonDocument.RootElement;

            string name = configRoot.GetProperty(nameof(Name)).GetString();
            string description = configRoot.GetProperty(nameof(Description)).GetString();
            string user = configRoot.GetProperty(nameof(User)).GetString();
            List<string> userParameters = configRoot.GetProperty(nameof(UserParameters)).Deserialize<List<string>>();
            List<CharacteristicWithModel> characteristics = configRoot.GetProperty(nameof(Characteristics)).Deserialize<List<CharacteristicWithModel>>();
            bool recalculateExisting = configRoot.GetProperty(nameof(RecalculateExisting)).GetBoolean();
            double dssSearchAccuracy = configRoot.GetProperty(nameof(DssSearchAccuracy)).GetDouble();

            JsonElement parametersElement = configRoot.GetProperty(nameof(Parameters));
            List<ParameterValue> parameters = ParameterValue.ListFromJsonElement(parametersElement);

            JsonElement algorithmElement = configRoot.GetProperty(nameof(OptimizationAlgorithm));
            OptimizationAlgorithm algorithm = OptimizationAlgorithm.FromJsonElement(algorithmElement);

            return new LaunchConfiguration(name, description, user, parameters, userParameters, 
                                           characteristics, algorithm, recalculateExisting, dssSearchAccuracy);
        }

        public static string ToJsonString(LaunchConfiguration config)
        {
            JsonObject configJsonObject = new()
            {
                [nameof(Name)] = config.Name,
                [nameof(Description)] = config.Description,
                [nameof(User)] = config.User,
                [nameof(Parameters)] = ParameterValue.ListToJsonArray(config.Parameters),
                [nameof(UserParameters)] = JsonNode.Parse(JsonSerializer.Serialize(config.UserParameters)),
                [nameof(Characteristics)] = JsonNode.Parse(JsonSerializer.Serialize(config.Characteristics)),
                [nameof(OptimizationAlgorithm)] = OptimizationAlgorithm.ToJsonNode(config.OptimizationAlgorithm),
                [nameof(RecalculateExisting)] = config.RecalculateExisting,
                [nameof(DssSearchAccuracy)] = config.DssSearchAccuracy
            };
            return configJsonObject.ToJsonString();
        }
    }
}
