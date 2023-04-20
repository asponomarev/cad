﻿using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;

namespace UhlnocsServer.Optimizations
{
    public sealed class SmartBinarySearch : OptimizationAlgorithm
    {
        public string ThroughputCharacteristic { get; set; }

        public int MaxIterations { get; set; }

        public double MaxRate { get; set; }

        public double Accuracy { get; set; }

        public double FirstValue { get; set; }

        public double LastValue { get; set; }

        public double CurrentRate { get; set; }

        public string FirstChangedBorder { get; set; }

        public bool BothBordersChanged { get; set; }

        public SmartBinarySearch(string variableParameter,
                             string throughputCharacteristic,
                             int maxIterations,
                             double maxRate,
                             double accuracy) : base(AlgorithmType.SmartBinarySearch, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            MaxIterations = maxIterations;
            MaxRate = maxRate;
            Accuracy = accuracy;
            LastValue = MaxRate;
            FirstChangedBorder = null;
            BothBordersChanged = false;
        }

        public List<ParameterValue> MakeCalculationParameters(List<ParameterValue> parameters, string variableParameterId)
        {
            List<ParameterValue> calculationParameters = new();
            double variableParameterValue = 0;
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
                        variableParameterValue = FirstValue;
                    }
                    else if (i == 1)
                    {
                        variableParameterValue = LastValue;
                    }
                    else
                    {
                        variableParameterValue = (FirstValue + LastValue) / 2;
                    }
                    CurrentRate = variableParameterValue;
                    calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                }
            }
            return calculationParameters;
        }

        public int MoveBorder(double throughput, int iteration)
        {
            if (iteration == 0)
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy) == false)
                {
                    return -1; // первая точка плохая
                }
            }
            else if (iteration == 1)
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy))
                {
                    return -2; // последняя точка хорошая
                }
            }
            else
            {
                if (IsPointGood(CurrentRate, throughput, Accuracy))
                {
                    FirstValue = CurrentRate;
                    if (FirstChangedBorder == null)
                    {
                        FirstChangedBorder = "Left";
                    }
                    else if (FirstChangedBorder == "Right")
                    {
                        BothBordersChanged = true;
                    }
                    else if (FirstChangedBorder == "Left" && BothBordersChanged == true)
                    {
                        return 0; // точка насыщения найдена
                    }
                }
                else
                {
                    LastValue = CurrentRate;
                    if (FirstChangedBorder == null)
                    {
                        FirstChangedBorder = "Right";
                    }
                    else if (FirstChangedBorder == "left")
                    {
                        BothBordersChanged = true;
                    }
                    else if (FirstChangedBorder == "Right" && BothBordersChanged == true)
                    {
                        return 0; // точка насыщения найдена
                    }
                }
            }
        }      
    }
}
