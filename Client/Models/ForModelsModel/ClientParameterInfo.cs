using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Parameters;
using UhlnocsServer.Models.Properties.Parameters.Infos;

namespace Client.Models.ForModelsModel
{
    public class ClientParameterInfo
    {
        private string _valueType { get; set; }

        public string ValueType
        {
            get { return _valueType; }
            set
            {
                _valueType = value;
                OnPropertyChanged(nameof(ValueType));
            }
        }

        private string _id { get; set; }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _name { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _description { get; set; }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private object _defaultValue { get; set; }

        public object DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                OnPropertyChanged(nameof(DefaultValue));
            }
        }

        private object? _minValue { get; set; }

        public object? MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                OnPropertyChanged(nameof(MinValue));
            }
        }

        private object? _maxValue { get; set; }

        public object? MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                OnPropertyChanged(nameof(MaxValue));
            }
        }

        private string? _possibleValues { get; set; }

        public string? PossibleValues
        {
            get { return _possibleValues; }
            set
            {
                _possibleValues = value;
                OnPropertyChanged(nameof(PossibleValues));
            }
        }

        public ClientParameterInfo(string valueType, string id, string name, string description, object defaultValue, object? minValue, object? maxValue, string? possibleValues)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public static ClientParameterInfo FromServerParameterInfo(ParameterInfo serverInfo)
        {
            PropertyValueType valueType = serverInfo.ValueType;
            if (serverInfo is IntParameterInfo intInfo)
            {
                return new(serverInfo.ValueType.ToString(), serverInfo.Id, serverInfo.Name, serverInfo.Description, intInfo.DefaultValue,
                           intInfo.MinValue, intInfo.MaxValue, null);
            }
            if (serverInfo is DoubleParameterInfo doubleInfo)
            {
                return new(serverInfo.ValueType.ToString(), serverInfo.Id, serverInfo.Name, serverInfo.Description, doubleInfo.DefaultValue,
                           doubleInfo.MinValue, doubleInfo.MaxValue, null);
            }
            if (serverInfo is BoolParameterInfo boolInfo)
            {
                return new(serverInfo.ValueType.ToString(), serverInfo.Id, serverInfo.Name, serverInfo.Description, boolInfo.DefaultValue,
                           null, null, null);
            }
            if (serverInfo is StringParameterInfo stringInfo)
            {
                return new(serverInfo.ValueType.ToString(), serverInfo.Id, serverInfo.Name, serverInfo.Description, stringInfo.DefaultValue,
                           null, null, String.Join(", ", stringInfo.PossibleValues));
            }

            return null;  //this should not happen
        }

        public static ParameterInfo ToServerParameterInfo(ClientParameterInfo clientInfo)
        {
            PropertyValueType valueType = MainWindow.StringToEnum<PropertyValueType>(clientInfo.ValueType);
            if (valueType == PropertyValueType.Int)
                return new IntParameterInfo(clientInfo.Id, clientInfo.Name, clientInfo.Description, 
                           (int)clientInfo.DefaultValue, (int)clientInfo.MinValue, (int)clientInfo.MaxValue);

            if (valueType == PropertyValueType.Double)
                return new DoubleParameterInfo(clientInfo.Id, clientInfo.Name, clientInfo.Description,
                           (double)clientInfo.DefaultValue, (double)clientInfo.MinValue, (double)clientInfo.MaxValue);

            if (valueType == PropertyValueType.Bool)
                return new BoolParameterInfo(clientInfo.Id, clientInfo.Name, clientInfo.Description,
                           (bool)clientInfo.DefaultValue);

            if (valueType == PropertyValueType.String)
                return new StringParameterInfo(clientInfo.Id, clientInfo.Name, clientInfo.Description,
                           (string)clientInfo.DefaultValue, new List<string>(clientInfo.PossibleValues.Split(", ")));

            return null;
        }
    }
}
