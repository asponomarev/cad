using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace UhlnocsServer.Models.Properties.Parameters.Infos
{
    public abstract class ModelParameterInfo : ModelPropertyInfo
    {
        public ModelParameterInfo(string id, string name, ModelPropertyValueType valueType,
                                  string description) : base(id, name, valueType, description)
        {

        }

        public static ModelParameterInfo FromJsonElement(JsonElement parameterInfoElement)
        {
            ModelPropertyValueType valueType = parameterInfoElement.GetProperty(nameof(ValueType)).Deserialize<ModelPropertyValueType>(PropertySerializerOptions);
            if (valueType == ModelPropertyValueType.Int)
            {
                return parameterInfoElement.Deserialize<IntModelParameterInfo>(PropertySerializerOptions);
            }
            if (valueType == ModelPropertyValueType.Double)
            {
                return parameterInfoElement.Deserialize<DoubleModelParameterInfo>(PropertySerializerOptions);
            }
            if (valueType == ModelPropertyValueType.Bool)
            {
                return parameterInfoElement.Deserialize<BoolModelParameterInfo>(PropertySerializerOptions);
            }
            return parameterInfoElement.Deserialize<StringModelParameterInfo>(PropertySerializerOptions);
        }

        public static JsonNode ToJsonNode(ModelParameterInfo parameterInfo)
        {
            string parameterInfoJsonString = JsonSerializer.Serialize(parameterInfo, parameterInfo.GetType(), PropertySerializerOptions);
            return JsonNode.Parse(parameterInfoJsonString);
        }

    }
}
