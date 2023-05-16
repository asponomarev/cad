using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Models
{
    public class Parameters : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string model { get; set; }
        public Parameters(int id, string name, string value, string model)
        {
            this.id = id;
            this.name = name;
            this.value = value;
            this.model = model;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
