using UhlnocsServer.Models.Properties.Parameters.Values;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Parameters.Infos;
using UhlnocsServer.Models;

namespace UhlnocsServer.Optimizations
{
    public sealed class ConstantStep : OptimizationAlgorithm
    {
        public double Step { get; set; }

        public int Iterations { get; set; }

        public StringParameterInfo? ParameterInfo { get; set; }

        public ConstantStep(string variableParameter, double step, int iterations) : base(AlgorithmType.ConstantStep, variableParameter)
        {
            Step = step;
            Iterations = iterations;
        }

        public List<ParameterValue> MakeCalculationParameters(List<ParameterValue> parameters, string variableParameterId, int iteration, 
                                                            PropertyValueType valueType, ParameterValue variableParameter, StringParameterInfo? parameterInfo)
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
                    if (valueType == PropertyValueType.String)
                    {
                        string variableParameterValue = parameterInfo.PossibleValues[iteration];
                        calculationParameters.Add(new StringParameterValue(variableParameterId, variableParameterValue));
                    }
                    else if (valueType == PropertyValueType.Bool)
                    {
                        bool variableParameterValue = (iteration == 0);
                        calculationParameters.Add(new BoolParameterValue(variableParameterId, variableParameterValue));
                    }
                    else if (valueType == PropertyValueType.Double)
                    {
                        double firstValue = (variableParameter as DoubleParameterValue).Value;
                        double variableParameterValue = firstValue + Step * iteration;
                        calculationParameters.Add(new DoubleParameterValue(variableParameterId, variableParameterValue));
                    }
                    else  // int
                    {
                        int firstValue = (variableParameter as IntParameterValue).Value;
                        int variableParameterValue = firstValue + (int)Math.Round(Step * iteration);
                        calculationParameters.Add(new IntParameterValue(variableParameterId, variableParameterValue));
                    }
                }
            }
            return calculationParameters;
        }
    }
}
