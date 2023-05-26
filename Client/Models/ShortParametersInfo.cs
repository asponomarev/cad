using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Models
{
    public class ShortParametersInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string valueType { get; set; }
        public string models { get; set; }
        public ShortParametersInfo(string id, string name, string valueType, string models)
        {
            this.id = id;
            this.name = name;
            this.valueType = valueType;
            this.models = models;
        }

    }
}
