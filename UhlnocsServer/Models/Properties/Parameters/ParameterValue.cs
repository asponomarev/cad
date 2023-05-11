using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Services;
using UhlnocsServer.Utils;

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

        public static string ListToJsonString(List<ParameterValue> parameters)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            JsonNode parametersNode = ListToJsonArray(parameters);
            return parametersNode.ToJsonString(options);
        }

        public static JsonDocument ListToJsonDocument(List<ParameterValue> parameters)
        {
            return JsonDocument.Parse(ListToJsonString(parameters));
        }

        public static List<ParameterValue> ListFromJsonDocument(JsonDocument document)
        {
            return ListFromJsonElement(document.RootElement);
        }

        public static ParameterValue GetFromListById(List<ParameterValue> parameters, string parameterId)
        {
            foreach (ParameterValue parameter in parameters)
            {
                if (parameter.Id == parameterId)
                {
                    return parameter;
                }
            }
            throw new Exception($"Parameters values list {parameters} has no parameter with id {parameterId}");
        }

        public static string ListToString(List<ParameterValue> parameters, string modelId)
        {
            StringBuilder builder = new();
            foreach (ParameterValue parameter in parameters)
            {
                if (parameter is IntParameterValue intParameter)
                {
                    builder.Append($"{intParameter.Id}={intParameter.Value},");
                }
                else if (parameter is DoubleParameterValue doubleParameter)
                {
                    builder.Append($"{doubleParameter.Id}={doubleParameter.Value},");
                }
                else if (parameter is BoolParameterValue boolParameter)
                {
                    builder.Append($"{boolParameter.Id}={boolParameter.Value},");
                }
                else if (parameter is StringParameterValue stringParameter)
                {
                    builder.Append($"{stringParameter.Id}={stringParameter.Value},");
                }
            }
            builder.Append($"modelId={modelId}");
            return builder.ToString();
        }

        public static string GetHashCode(List<ParameterValue> parameters, string modelId) 
        {
            string parametersString = ListToString(parameters, modelId);
            return HashUtils.GetHashCode(parametersString);
        }

        public static object? GetValue(ParameterValue param)
        {
            if (param is IntParameterValue intParam)
            {
                return intParam.Value;
            }
            if (param is DoubleParameterValue doubleParam)
            {
                return doubleParam.Value;
            }
            if (param is BoolParameterValue boolParam)
            {
                return boolParam.Value;
            }
            if (param is IntParameterValue stringParam)
            {
                return stringParam.Value;
            }
            return null;
        }
    }
}
