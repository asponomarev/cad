using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UhlnocsServer.Models.Properties.Parameters.Infos;

namespace UhlnocsServer.Models.Properties.Parameters
{
    public abstract class ParameterInfo : PropertyInfo
    {
        public ParameterInfo(string id, string name, PropertyValueType valueType,
                             string description) : base(id, name, valueType, description)
        {

        }

        public static ParameterInfo FromJsonElement(JsonElement parameterInfoElement)
        {
            PropertyValueType valueType = parameterInfoElement.GetProperty(nameof(ValueType)).Deserialize<PropertyValueType>(PropertySerializerOptions);
            if (valueType == PropertyValueType.Int)
            {
                return parameterInfoElement.Deserialize<IntParameterInfo>(PropertySerializerOptions);
            }
            if (valueType == PropertyValueType.Double)
            {
                return parameterInfoElement.Deserialize<DoubleParameterInfo>(PropertySerializerOptions);
            }
            if (valueType == PropertyValueType.Bool)
            {
                return parameterInfoElement.Deserialize<BoolParameterInfo>(PropertySerializerOptions);
            }
            return parameterInfoElement.Deserialize<StringParameterInfo>(PropertySerializerOptions);
        }

        public static JsonNode ToJsonNode(ParameterInfo parameterInfo)
        {
            string parameterInfoJsonString = JsonSerializer.Serialize(parameterInfo, parameterInfo.GetType(), PropertySerializerOptions);
            return JsonNode.Parse(parameterInfoJsonString);
        }

    }
}
