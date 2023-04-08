using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Optimizations
{
    public abstract class OptimizationAlgorithm
    {
        public AlgorithmType Type { get; set; }

        public string VariableParameter { get; set; }

        public OptimizationAlgorithm(AlgorithmType type, string variableParameter)
        {
            Type = type;
            VariableParameter = variableParameter;
        }

        public static OptimizationAlgorithm FromJsonElement(JsonElement algorithmElement)
        {
            AlgorithmType type = algorithmElement.GetProperty(nameof(Type)).Deserialize<AlgorithmType>(PropertyBase.PropertySerializerOptions);

            // later will be changed to "if type == a..., if type == b..."
            return algorithmElement.Deserialize<ConstantStep>(PropertyBase.PropertySerializerOptions);
        }

        public static JsonNode ToJsonNode(OptimizationAlgorithm algorithm)
        {
            string algorithmJsonString = JsonSerializer.Serialize(algorithm, algorithm.GetType(), PropertyBase.PropertySerializerOptions);
            return JsonNode.Parse(algorithmJsonString);
        }
    }
}
