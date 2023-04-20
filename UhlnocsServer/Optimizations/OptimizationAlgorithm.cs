﻿using System.Text.Json;
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

        public static bool IsPointGood(double X, double Y, double Accuracy)
        {
            return Math.Min(X, Y) / Math.Max(X, Y) > Accuracy;
        }

        public static OptimizationAlgorithm FromJsonElement(JsonElement algorithmElement)
        {
            AlgorithmType type = algorithmElement.GetProperty(nameof(Type)).Deserialize<AlgorithmType>(PropertyBase.PropertySerializerOptions);

            if (type == AlgorithmType.SmartConstantStep)
            {
                return algorithmElement.Deserialize<SmartConstantStep>(PropertyBase.PropertySerializerOptions);
            }
            else if (type == AlgorithmType.BinarySearch)
            {
                return algorithmElement.Deserialize<BinarySearch>(PropertyBase.PropertySerializerOptions);
            }
            else if (type == AlgorithmType.SmartBinarySearch)
            {
                return algorithmElement.Deserialize<SmartBinarySearch>(PropertyBase.PropertySerializerOptions);
            }
            else if (type == AlgorithmType.GoldenSection)
            {
                return algorithmElement.Deserialize<GoldenSection>(PropertyBase.PropertySerializerOptions);
            }
            else if (type == AlgorithmType.SmartGoldenSection)
            {
                return algorithmElement.Deserialize<SmartGoldenSection>(PropertyBase.PropertySerializerOptions);
            }
            else // regular constant step
            {
                return algorithmElement.Deserialize<ConstantStep>(PropertyBase.PropertySerializerOptions);
            }
        }

        public static string ToJsonString(OptimizationAlgorithm algorithm)
        {
            return JsonSerializer.Serialize(algorithm, algorithm.GetType(), PropertyBase.PropertySerializerOptions);
        }

        public static JsonNode ToJsonNode(OptimizationAlgorithm algorithm)
        {
            string algorithmJsonString = ToJsonString(algorithm);
            return JsonNode.Parse(algorithmJsonString);
        }
    }
}
