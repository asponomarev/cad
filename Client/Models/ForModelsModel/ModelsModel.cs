using Client.Models.ForModelsModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;

namespace Client.Models
{
    public class ModelsModel : INotifyPropertyChanged
    {
        public static Dictionary<string, ParameterWithModels> _parametersWithModels = new();

        public static Dictionary<string, CharacteristicWithModels> _characteristicsWithModels = new();

        public static Dictionary<string, ClientModelConfiguration> _modelConfigurations = new();

        public ClientModelConfiguration _modelConfiguration { get; set; } = new();

        public List<ShortModelInfo> _shortModelInfos { get; set; } = new();

        public ShortModelInfo _modelsFilter { get; set; }

        public List<string> _options { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
