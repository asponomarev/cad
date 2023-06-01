using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;
using Microsoft.VisualBasic;

namespace UhlnocsServer.Optimizations
{
    public sealed class SmartConstantStep : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public double Step { get; set; }

        public int MaxIterations { get; set; }

        public double Accuracy { get; set; }

        public double FirstValue { get; set; }

        public double CurrentRate { get; set; }

        public SmartConstantStep (string variableParameter,
                                  string throughputCharacteristic,
                                  double step,
                                  int maxIterations = 15,
                                  double accuracy = 0.9) : base(AlgorithmType.SmartConstantStep, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            Step = step;
            MaxIterations = maxIterations;
            Accuracy = accuracy;
        }

        public List<ParameterValue> MakeCalculationParameters(List<ParameterValue> parameters, string variableParameterId, int iteration)
        {
            List<ParameterValue> calculationParameters = new();
            foreach (ParameterValue parameter in parameters)
            {
                if (parameter.Id != variableParameterId)
                {
                    calculationParameters.Add(parameter);
                }
                else
                {
                    CurrentRate = FirstValue + Step * iteration;
                    calculationParameters.Add(new DoubleParameterValue(variableParameterId, CurrentRate));
                }
            }
            return calculationParameters;
        }

        public AlgorithmStatus CheckPointIsGood(double throughput, int iteration)
        {
            if (iteration == 0)
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy) == false)
                {
                    return AlgorithmStatus.FirstPointIsBad;
                }
            }
            if (IsPointGood(CurrentRate, throughput, Accuracy))
            {
                return AlgorithmStatus.Calculating;
            }
            return AlgorithmStatus.FoundSaturationPoint;
        }
    }
}
