using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Services;

namespace UhlnocsServer.Models.Properties.Parameters
{
    public abstract class ParameterValue : PropertyValue
    {
        public ParameterValue(string id) : base(id)
        {

        }

        public static List<ParameterValue> ListFromJsonElement(JsonElement parametersElement)
        {
            List<ParameterValue> parameters = new();
            foreach (JsonElement parameterElement in parametersElement.EnumerateArray())
            {               
                parameters.Add(FromJsonElement(parameterElement));
            }
            return parameters;
        }

        public static ParameterValue FromJsonElement(JsonElement parameterElement)
        {
            string parameterId = parameterElement.GetProperty(nameof(Id)).GetString();
            PropertyValueType parameterValueType = ModelService.ParametersWithModels[parameterId].ValueType;
            if (parameterValueType == PropertyValueType.Int)
            {
                return parameterElement.Deserialize<IntParameterValue>(PropertySerializerOptions);
            }
            if (parameterValueType == PropertyValueType.Double)
            {
                return parameterElement.Deserialize<DoubleParameterValue>(PropertySerializerOptions);
            }
            if (parameterValueType == PropertyValueType.Bool)
            { 
                return parameterElement.Deserialize<BoolParameterValue>(PropertySerializerOptions);
            }
            return parameterElement.Deserialize<StringParameterValue>(PropertySerializerOptions);
        }

        public static JsonArray ListToJsonArray(List<ParameterValue> parameters)
        {
            JsonArray parametersArray = new();
            foreach (ParameterValue parameter in parameters)
            {
                parametersArray.Add(ToJsonNode(parameter));
            }
            return parametersArray;
        }

        public static JsonNode ToJsonNode(ParameterValue parameterValue)
        {
            string parameterValueJsonString = JsonSerializer.Serialize(parameterValue, parameterValue.GetType(), PropertySerializerOptions);
            return JsonNode.Parse(parameterValueJsonString);
        }
    }
}
