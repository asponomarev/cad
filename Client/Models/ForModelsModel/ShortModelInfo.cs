using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.ForModelsModel
{
    public class ShortModelInfo
    {
        public string _id { get; set; }

        public string _name { get; set; }

        public ShortModelInfo(string id, string name)
        {
            _id = id;
            _name = name;

        }

    }
}
