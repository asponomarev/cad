﻿using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;

namespace UhlnocsServer.Optimizations
{
    public sealed class SmartGoldenSection : OptimizationAlgorithm
    {
        public const double PHI_number = 1.618;

        public string ThroughputCharacteristic { get; set; }

        public int MaxIterations { get; set; }

        public double MaxRate { get; set; }

        public double Accuracy { get; set; }

        public double FirstValue { get; set; }

        public double LastValue { get; set; }

        public double CurrentRate { get; set; }

        public double X1 { get; set; }

        public double X2 { get; set; }

        public string LastFoundPoint { get; set; }

        public string NextPoint { get; set; }

        public bool InMiddleSegment { get; set; }

        public SmartGoldenSection(string variableParameter,
                             string throughputCharacteristic,
                             int maxIterations,
                             double maxRate,
                             double accuracy = 0.9) : base(AlgorithmType.SmartGoldenSection, variableParameter)
        {
            ThroughputCharacteristic = throughputCharacteristic;
            MaxIterations = maxIterations;
            MaxRate = maxRate;
            Accuracy = accuracy;
            LastValue = MaxRate;
            NextPoint = "X1";
            InMiddleSegment = false;
        }

        public List<ParameterValue> MakeCalculationParameters(List<ParameterValue> parameters, string variableParameterId)
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
                    else if (i == 1)
                    {
                        CurrentRate = LastValue;
                    }
                    else
                    {
                        if (NextPoint == "X1")
                        {
                            X1 = LastValue - (LastValue - FirstValue) / PHI_number;
                            CurrentRate = X1;
                            LastFoundPoint == "X1";
                            NextPoint = "X2";
                        }
                        else // NextPoint = X2
                        {
                            X2 = FirstValue + (LastValue - FirstValue) / PHI_number;
                            CurrentRate = X2;
                            LastFoundPoint = "X2";
                            NextPoint = "X1";
                        }
                    }             
                    calculationParameters.Add(new DoubleParameterValue(variableParameterId, CurrentRate));
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
                if (LastFoundPoint == "X1")
                {
                    if (IsPointGood(CurrentRate, throughput, Accuracy) == false)
                    {
                        LastValue = X1;
                        NextPoint = "X1";
                        if (InMiddleSegment == true)
                        {
                            return 0; // точка насыщения найдена
                        }
                    }
                }
                else // LastFoundPoint == X2
                {
                    if (IsPointGood(CurrentRate, throughput, Accuracy))
                    {
                        FirstValue = X2;
                        if (InMiddleSegment == true)
                        {
                            return 0; // точка насыщения найдена
                        }
                    }
                    else // X2 - bad
                    {
                        FirstValue = X1;
                        LastValue = X2;
                        InMiddleSegment = true;
                    }
                }
            }            
        }
    }
}