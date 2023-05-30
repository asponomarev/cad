using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UhlnocsServer.Models.Properties;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Characteristics.Infos;

namespace Client.Models.ForModelsModel
{
    public class ClientCharacteristicInfo
    {
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


        public ClientCharacteristicInfo(string id, string name, string description, string valueType)
        {
            _description = description;
            _id = id;
            _name = name;   
            _valueType = valueType;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public static ClientCharacteristicInfo FromServerCharacteristicInfo(CharacteristicInfo serverInfo)
        {
            return new(serverInfo.Id, serverInfo.Name, serverInfo.Description, serverInfo.ValueType.ToString());
        }

        public static CharacteristicInfo ToServerCharacteristicInfo(ClientCharacteristicInfo clientInfo)
        {
            PropertyValueType valueType = MainWindow.StringToEnum<PropertyValueType>(clientInfo.ValueType);
            if (valueType == PropertyValueType.Int)
                return new IntCharacteristicInfo(clientInfo.Id, clientInfo.Name, clientInfo.Description);
            return new DoubleCharacteristicInfo(clientInfo.Id, clientInfo.Name, clientInfo.Description);
        }

    }
}
