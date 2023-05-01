using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;

namespace UhlnocsServer.Optimizations
{
    public sealed class BinarySearch : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public int Iterations { get; set; }

        public double MaxRate { get; set; }

        public double Accuracy { get; set; }

        public double FirstValue { get; set; }

        public double LastValue { get; set; }

        public double CurrentRate { get; set; }

        public BinarySearch (string variableParameter,
                             string throughputCharacteristic,
                             int iterations,
                             double maxRate,
                             double accuracy = 0.9) : base(AlgorithmType.BinarySearch, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            Iterations = iterations;
            MaxRate = maxRate;
            Accuracy = accuracy;
            LastValue = MaxRate;
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
                    if (iteration == 0)
                    {
                        CurrentRate = FirstValue;
                    }
                    else if (iteration == 1)
                    {
                        CurrentRate = LastValue;
                    }
                    else
                    {
                        CurrentRate = (FirstValue + LastValue) / 2;
                    }
                    calculationParameters.Add(new DoubleParameterValue(variableParameterId, CurrentRate));
                }
            }
            return calculationParameters;
        }

        public AlgorithmStatus MoveBorder(double throughput, int iteration)
        {
            if (iteration == 0)
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy) == false)
                {
                    return AlgorithmStatus.FirstPointIsBad;
                }
            }
            else if (iteration == 1)
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy))
                {
                    return AlgorithmStatus.LastPointIsGood;
                }
            }
            else
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy))
                {
                    FirstValue = CurrentRate;
                }
                else
                {
                    LastValue = CurrentRate;
                }
            }
            return AlgorithmStatus.FoundSaturationPoint;
        }
    }
}
