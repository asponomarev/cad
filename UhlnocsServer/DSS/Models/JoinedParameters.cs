using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace DSS.Models
{
    internal class JoinedParameters
    {
        internal JoinedParameters(ParameterValue first, ParameterValue second) 
        {
            First = first;
            Second = second;
        }

        public ParameterValue First {get;set;}
        public ParameterValue Second { get; set; }

        public bool CompareParameters(double searchAccuracy)
        {
            if (First is IntParameterValue firstIntParameter
                && Second is IntParameterValue secondIntParameter)
            {
                return (secondIntParameter.Value >= firstIntParameter.Value - firstIntParameter.Value * searchAccuracy)
                    && (secondIntParameter.Value <= firstIntParameter.Value + firstIntParameter.Value * searchAccuracy);
            }
            else if (First is DoubleParameterValue firstDoubleParameter
                && Second is DoubleParameterValue secondDoubleParameter)
            {
                return (secondDoubleParameter.Value >= firstDoubleParameter.Value - firstDoubleParameter.Value * searchAccuracy)
                    && (secondDoubleParameter.Value <= firstDoubleParameter.Value + firstDoubleParameter.Value * searchAccuracy);
            }
            else if (First is BoolParameterValue firstBoolParameter
                && Second is BoolParameterValue secondBoolParameter)
            {
                return firstBoolParameter.Value == secondBoolParameter.Value;
            }
            else if (First is StringParameterValue firstStringParameter
                && Second is StringParameterValue secondStringParameter)
            {
                return string.Equals(firstStringParameter.Value, secondStringParameter.Value);
            }
            return false;
        }
    }
}
