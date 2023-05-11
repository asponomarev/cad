using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UhlnocsServer.Models.Properties;

namespace Client.Models.ForModelsModel
{
    public class ClientCharacteristicInfo
    {
        public string _id { get; set; }
        public string _name { get; set; }
        public string _description { get; set; }
        public PropertyValueType _valueType { get; set; }

        public ClientCharacteristicInfo(string id, string name, string description, PropertyValueType valueType)
        {
            _description = description;
            _id = id;
            _name = name;   
            _valueType = valueType;
        }


    }
}
