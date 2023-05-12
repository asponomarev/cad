using UhlnocsServer.Optimizations;

namespace UhlnocsServer.Calculations.LaunchResult
{
    // this class is a part of LaunchResult object and contains info about optimization algorithm
    public class OptimizationAlgorithmInfo
    {
        public AlgorithmType AlgorithmType { get; set; }

        public string VariableParameterId { get; set; }

        public double? Step { get; set; }

        public int? Iterations { get; set; }

        public int? MaxIterations { get; set; }

        public double? Accuracy { get; set; }

        public string? ThroughputCharacteristicId { get; set; }

        public OptimizationAlgorithmInfo(AlgorithmType algorithmType, string variableParameterId,
                                         double? step, int? iterations, int? maxIterations,
                                         double? accuracy, string? throughputCharacteristicId)
        {
            AlgorithmType = algorithmType;
            VariableParameterId = variableParameterId;
            Step = step;
            Iterations = iterations;
            MaxIterations = maxIterations;
            Accuracy = accuracy;
            ThroughputCharacteristicId = throughputCharacteristicId;
        }
    }
}
