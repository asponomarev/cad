using UhlnocsServer.Models.Properties;

namespace UhlnocsServer.Calculations.LaunchResult
{
    public class CharacteristicResult
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public PropertyValueType ValueType { get; set; }

        public string CalculatedByModel { get; set; }

        public bool ModelSetByUser { get; set; }

        List<object> Values { get; set; }

        public CharacteristicResult(string id, string name, PropertyValueType valueType,
                                    string calculatedByModel, bool modelSetByUser, List<object> values)
        {
            Id = id;
            Name = name;
            ValueType = valueType;
            CalculatedByModel = calculatedByModel;
            ModelSetByUser = modelSetByUser;
            Values = values;
        }
    }
}
