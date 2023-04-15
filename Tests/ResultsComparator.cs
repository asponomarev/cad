using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Values;

namespace Tests
{
    public static class ResultsComparator
    {
        public static bool IsEquals(List<ParameterValue> firstParams, List<ParameterValue> secondParams)
        {
            var isEquals = false;
            foreach (var param in firstParams)
            {
                if (param is IntParameterValue firstInt && secondParams.FirstOrDefault(p => string.Equals(p.Id, param.Id)) is IntParameterValue secondInt)
                {
                    isEquals = string.Equals(firstInt.Id, secondInt.Id)
                        && firstInt.Value == secondInt.Value;
                }
                else if (param is DoubleParameterValue firstDouble && secondParams.FirstOrDefault(p => string.Equals(p.Id, param.Id)) is DoubleParameterValue secondDouble)
                {
                    isEquals = string.Equals(firstDouble.Id, secondDouble.Id)
                        && firstDouble.Value == secondDouble.Value;
                }
                else if (param is BoolParameterValue firstBool && secondParams.FirstOrDefault(p => string.Equals(p.Id, param.Id)) is BoolParameterValue secondBool)
                {
                    isEquals = string.Equals(firstBool.Id, secondBool.Id)
                        && firstBool.Value == secondBool.Value;
                }
                else if (param is StringParameterValue firstString && secondParams.FirstOrDefault(p => string.Equals(p.Id, param.Id)) is StringParameterValue secondString)
                {
                    isEquals = string.Equals(firstString.Id, secondString.Id)
                        && string.Equals(firstString.Value, secondString.Value);
                }

                if (!isEquals)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEquals(List<CharacteristicWithModel> firstCharacteristics, List<CharacteristicWithModel> secondCharacteristics)
        {
            for (var i = 1; i < firstCharacteristics.Count; i++)
            {
                if (!(string.Equals(firstCharacteristics[i].Id, secondCharacteristics[i].Id) 
                    && string.Equals(firstCharacteristics[i].Model, secondCharacteristics[i].Model)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
