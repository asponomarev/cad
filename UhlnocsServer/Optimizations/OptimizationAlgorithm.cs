using System.Text.Json;
using System.Text.Json.Nodes;
using UhlnocsServer.Calculations;
using UhlnocsServer.Calculations.LaunchResult;
using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Optimizations
{
    public abstract class OptimizationAlgorithm
    {
        public AlgorithmType Type { get; set; }

        public string VariableParameter { get; set; }

        public Dictionary<string, AlgorithmStatus> ModelsAlgorithmsStatuses { get; set; }

        public OptimizationAlgorithm(AlgorithmType type, string variableParameter)
        {
            Type = type;
            VariableParameter = variableParameter;
            ModelsAlgorithmsStatuses = new Dictionary<string, AlgorithmStatus>();
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

        public static JsonDocument ToJsonDocument(OptimizationAlgorithm algorithm)
        {
            string jsonString = ToJsonString(algorithm);
            return JsonDocument.Parse(jsonString);
        }

        public static JsonDocument UpdateModelsAlgorithmsStatuses(OptimizationAlgorithm originalAlgorithm,
                                                                  Task<ModelAndAlgorithmStatuses>[] modelsTasks)
        {
            foreach (Task<ModelAndAlgorithmStatuses> task in modelsTasks)
            {
                originalAlgorithm.ModelsAlgorithmsStatuses.Add(task.Result.ModelId, task.Result.AlgorithmStatus);
            }
            return ToJsonDocument(originalAlgorithm);
        }

        // TODO: add min and max values to result class
        public static OptimizationAlgorithmInfo ToInfo(OptimizationAlgorithm algorithm)
        {
            if (algorithm is ConstantStep cs)
            {
                return new OptimizationAlgorithmInfo(cs.Type, cs.VariableParameter, cs.Step, cs.Iterations, null, null, null);
            }
            else if (algorithm is SmartConstantStep scs)
            {
                return new OptimizationAlgorithmInfo(scs.Type, scs.VariableParameter, scs.Step, null,
                                                     scs.MaxIterations, scs.Accuracy, scs.ThroughputCharacteristic);
            }
            else if (algorithm is BinarySearch bs)
            {
                return new OptimizationAlgorithmInfo(bs.Type, bs.VariableParameter, null, bs.Iterations,
                                                     null, bs.Accuracy, bs.ThroughputCharacteristic);
            }

            else if (algorithm is SmartBinarySearch sbs)
            {
                return new OptimizationAlgorithmInfo(sbs.Type, sbs.VariableParameter, null, null,
                                                     sbs.MaxIterations, sbs.Accuracy, sbs.ThroughputCharacteristic);
            }
            else if (algorithm is GoldenSection gs)
            {
                return new OptimizationAlgorithmInfo(gs.Type, gs.VariableParameter, null, gs.Iterations,
                                                     null, gs.Accuracy, gs.ThroughputCharacteristic);
            }
            else if (algorithm is SmartGoldenSection sgs)
            {
                return new OptimizationAlgorithmInfo(sgs.Type, sgs.VariableParameter, null, null,
                                                     sgs.MaxIterations, sgs.Accuracy, sgs.ThroughputCharacteristic);
            }
            return null;  // this should never happen
        }
    }
}
