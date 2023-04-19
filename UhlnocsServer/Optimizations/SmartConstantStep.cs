﻿using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;

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
                                  int maxIterations,
                                  double accuracy) : base(AlgorithmType.SmartConstantStep, variableParameter)
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
                    calculationParameters.Add(parameter);  // this may be bad
                }
                else
                {
                    if (i == 0)
                    {
                        CurrentRate = FirstValue;
                    }
                    else
                    {
                        CurrentRate = FirstValue + Step * iteration;
                    }
                    calculationParameters.Add(new DoubleParameterValue(variableParameterId, CurrentRate));
                }
            }
            return calculationParameters;
        }

        public bool CheckPointIsGood(double throughput)
        {
            if (IsPointGood(CurrentRate, throughput, Accuracy))
            {
                return false;
            }
            return true;
        }
    }
}
