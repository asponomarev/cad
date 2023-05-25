using Client.Models.ForModelsModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
