using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace DSS.Extensions
{
    internal static class ParameterValueExtensions
    {
        public static object GetValue(this ParameterValue parameter)
        {
            return parameter switch
            {
                IntParameterValue intParameter => intParameter.Value,
                DoubleParameterValue doubleParameter => doubleParameter.Value,
                BoolParameterValue boolParameter => boolParameter.Value,
                StringParameterValue stringParameter => stringParameter.Value,
                _ => throw new Exception("Unknown parameter type")
            };
        }
    }
}
