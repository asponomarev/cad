using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Infos;

namespace UhlnocsServer.Models
{
    // this class contains info about model and is also used in messages sent via gRpc
    public class ModelConfiguration
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ModelFilePath { get; set; }

        public int ModelOkExitCode { get; set; }

        public string ModelArgumentsFormatString { get; set; }

        public string PreparerFilePath { get; set; }

        public int PreparerOkExitCode { get; set; }

        public string CollectorFilePath { get; set; }

        public int CollectorOkExitCode { get; set; }

        public bool CollectFromStdout { get; set; }

        public List<string> ResponsibleUsers { get; set; }

        public List<ParameterInfo> ParametersInfo { get; set; }

        public List<CharacteristicInfo> CharacteristicsInfo { get; set; }

        public ModelConfiguration(string id,
                                  string name,
                                  string description,
                                  string modelFilePath,
                                  int modelOkExitCode,
                                  string modelArgumentsFormatString,
                                  string preparerFilePath,
                                  int preparerOkExitCode,
                                  string collectorFilePath,
                                  int collectorOkExitCode,
                                  bool collectFromStdout,
                                  List<string> responsibleUsers,
                                  List<ParameterInfo> parametersInfo,
                                  List<CharacteristicInfo> characteristicsInfo)
        {
            Id = id;
            Name = name;
            Description = description;
            
            ModelFilePath = modelFilePath;
            ModelOkExitCode = modelOkExitCode;
            ModelArgumentsFormatString = modelArgumentsFormatString;

            PreparerFilePath = preparerFilePath;
            PreparerOkExitCode = preparerOkExitCode;

            CollectorFilePath = collectorFilePath;
            CollectorOkExitCode = collectorOkExitCode;
            CollectFromStdout = collectFromStdout;

            ResponsibleUsers = responsibleUsers;
            ParametersInfo = parametersInfo;
            CharacteristicsInfo = characteristicsInfo;
        }

        public StringParameterInfo GetStringParameterInfo(string parameterId)
        {
            foreach (ParameterInfo parameter in ParametersInfo)
            {
                if (parameter.Id == parameterId)
                {
                    return (StringParameterInfo)parameter;
                }
            }
            throw new Exception($"Model {Id} has no parameter with id {parameterId}");
        }

        public static ModelConfiguration FromJsonDocument(JsonDocument configJsonDocument)
        {
            JsonElement configRoot = configJsonDocument.RootElement;

            string id = configRoot.GetProperty(nameof(Id)).GetString();
            string name = configRoot.GetProperty(nameof(Name)).GetString();
            string description = configRoot.GetProperty(nameof(Description)).GetString();
            string modelFilePath = configRoot.GetProperty(nameof(ModelFilePath)).GetString();
            int modelOkExitCode = configRoot.GetProperty(nameof(ModelOkExitCode)).GetInt32();
            string modelArgumentsFormatString = configRoot.GetProperty(nameof(ModelArgumentsFormatString)).GetString();
            string preparerFilePath = configRoot.GetProperty(nameof(PreparerFilePath)).GetString();
            int preparerOkExitCode = configRoot.GetProperty(nameof(PreparerOkExitCode)).GetInt32();
            string collectorFilePath = configRoot.GetProperty(nameof(CollectorFilePath)).GetString();
            int collectorOkExitCode = configRoot.GetProperty(nameof(CollectorOkExitCode)).GetInt32();
            bool collectFromStdout = configRoot.GetProperty(nameof(CollectFromStdout)).GetBoolean();
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

            return new ModelConfiguration(id, name, description,
                                          modelFilePath, modelOkExitCode, modelArgumentsFormatString,
                                          preparerFilePath, preparerOkExitCode,
                                          collectorFilePath, collectorOkExitCode, collectFromStdout,
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
                [nameof(ModelOkExitCode)] = config.ModelOkExitCode,
                [nameof(ModelArgumentsFormatString)] = config.ModelArgumentsFormatString,
                [nameof(PreparerFilePath)] = config.PreparerFilePath,
                [nameof(PreparerOkExitCode)] = config.PreparerOkExitCode,
                [nameof(CollectorFilePath)] = config.CollectorFilePath,
                [nameof(CollectorOkExitCode)] = config.CollectorOkExitCode,
                [nameof(CollectFromStdout)] = config.CollectFromStdout,
                [nameof(ResponsibleUsers)] = responsibleUsersJsonArray,
                [nameof(ParametersInfo)] = parametersJsonArray,
                [nameof(CharacteristicsInfo)] = characteristicsJsonArray
            };
            return configJsonObject.ToJsonString();
        }
    }
}
