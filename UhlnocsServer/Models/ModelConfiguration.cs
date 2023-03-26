using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;

namespace UhlnocsServer.Models
{
    public class ModelConfiguration
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ModelFilePath { get; set; }

        public string PreparerFilePath { get; set; }

        public string CollectorFilePath { get; set; }

        public List<string> ResponsibleUsers { get; set; }

        public List<ParameterInfo> ParametersInfo { get; set; }

        public List<CharacteristicInfo> CharacteristicsInfo { get; set; }

        public ModelConfiguration(string id,
                                  string name,
                                  string description,
                                  string modelFilePath,
                                  string preparerFilePath,
                                  string collectorFilePath,
                                  List<string> responsibleUsers,
                                  List<ParameterInfo> parametersInfo,
                                  List<CharacteristicInfo> characteristicsInfo)
        {
            Id = id;
            Name = name;
            Description = description;
            ResponsibleUsers = responsibleUsers;
            ModelFilePath = modelFilePath;
            PreparerFilePath = preparerFilePath;
            CollectorFilePath = collectorFilePath;
            ParametersInfo = parametersInfo;
            CharacteristicsInfo = characteristicsInfo;
        }

        public static ModelConfiguration FromJsonDocument(JsonDocument configJsonDocument)
        {
            JsonElement configRoot = configJsonDocument.RootElement;

            string id = configRoot.GetProperty(nameof(Id)).GetString();
            string name = configRoot.GetProperty(nameof(Name)).GetString();
            string description = configRoot.GetProperty(nameof(Description)).GetString();
            string modelFilePath = configRoot.GetProperty(nameof(ModelFilePath)).GetString();
            string preparerFilePath = configRoot.GetProperty(nameof(PreparerFilePath)).GetString();
            string collectorFilePath = configRoot.GetProperty(nameof(CollectorFilePath)).GetString();
            List<string> responsibleUsers = configRoot.GetProperty(nameof(ResponsibleUsers)).Deserialize<List<string>>();

            JsonElement parametersInfoElement = configRoot.GetProperty(nameof(ParametersInfo));
            List<ParameterInfo> parametersInfo = new();
            foreach (JsonElement parameterInfoElement in parametersInfoElement.EnumerateArray())
            {
                parametersInfo.Add(ParameterInfo.FromJsonElement(parameterInfoElement));
            }

            JsonElement characteristicsInfoElement = configRoot.GetProperty(nameof(CharacteristicsInfo));
            List<CharacteristicInfo> characteristicsInfo = new();
            foreach(JsonElement characteristicInfoElement in characteristicsInfoElement.EnumerateArray())
            {
                characteristicsInfo.Add(CharacteristicInfo.FromJsonElement(characteristicInfoElement));
            }

            return new ModelConfiguration(id, name, description, modelFilePath, preparerFilePath, collectorFilePath,
                                          responsibleUsers, parametersInfo, characteristicsInfo);
        }

        public static string ToJsonString(ModelConfiguration config)
        {
            JsonArray responsibleUsersJsonArray = new();
            foreach (string userId in config.ResponsibleUsers)
            {
                responsibleUsersJsonArray.Add(JsonNode.Parse(JsonSerializer.Serialize(userId)));
            }

            JsonArray parametersJsonArray = new();
            foreach (ParameterInfo parameterInfo in config.ParametersInfo)
            {
                parametersJsonArray.Add(ParameterInfo.ToJsonNode(parameterInfo));
            }

            JsonArray characteristicsJsonArray = new();
            foreach (CharacteristicInfo characteristicInfo in config.CharacteristicsInfo)
            {
                characteristicsJsonArray.Add(CharacteristicInfo.ToJsonNode(characteristicInfo));
            }

            JsonObject configJsonObject = new()
            {
                [nameof(Id)] = config.Id,
                [nameof(Name)] = config.Name,
                [nameof(Description)] = config.Description,
                [nameof(ModelFilePath)] = config.ModelFilePath,
                [nameof(PreparerFilePath)] = config.PreparerFilePath,
                [nameof(CollectorFilePath)] = config.CollectorFilePath,
                [nameof(ResponsibleUsers)] = responsibleUsersJsonArray,
                [nameof(ParametersInfo)] = parametersJsonArray,
                [nameof(CharacteristicsInfo)] = characteristicsJsonArray
            };
            return configJsonObject.ToJsonString();
        }
    }
}
