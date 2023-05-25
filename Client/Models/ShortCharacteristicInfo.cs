using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Client.Models
{
    public class ShortCharacteristicInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string valueType { get; set; }
        public string models { get; set; }
        public ShortCharacteristicInfo(string id, string name, string valueType, string models)
        {
            this.id = id;
            this.name = name;
            this.valueType = valueType;
            this.models = models;
        }

    }
}
