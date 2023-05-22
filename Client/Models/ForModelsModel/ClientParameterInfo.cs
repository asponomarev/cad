using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UhlnocsServer.Models.Properties;

namespace Client.Models.ForModelsModel
{
    public class ClientParameterInfo
    {
        public PropertyValueType _valueType { get; set; }
        public string _id { get; set; }
        public string _name { get; set; }
        public string _description { get; set; }
        public object _defaultValue { get; set; }

        public object? _minValue { get; set; }

        public object? _maxValue { get; set; }

        public List<string>? _possibleValues { get; set; }

        public ClientParameterInfo(PropertyValueType valueType, string id, string name, string description, object defaultValue, object? minValue, object? maxValue, List<string>? possibleValues)
        {
            _valueType = valueType;
            _id = id;
            _name = name;
            _description = description;
            _defaultValue = defaultValue;
            _minValue = minValue;
            _maxValue = maxValue;
            _possibleValues = possibleValues;
        }
    }
}
