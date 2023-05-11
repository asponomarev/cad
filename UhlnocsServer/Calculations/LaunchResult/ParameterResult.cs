using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Calculations.LaunchResult
{
    public class ParameterResult
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public PropertyValueType ValueType { get; set; }

        public List<string> UsedByModels { get; set; }

        public bool ValueSetByUser { get; set; }

        List<object> Values { get; set; }

        public ParameterResult(string id, string name, PropertyValueType valueType, List<string> usedByModels,
                               bool valueSetByUser, List<object> values)
        {
            Id = id;
            Name = name;
            ValueType = valueType;
            UsedByModels = usedByModels;
            ValueSetByUser = valueSetByUser;
            Values = values;
        }
    }
}
