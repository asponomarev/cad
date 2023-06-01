using Client.Models.ForModelsModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        private ObservableCollection<ShortModelInfo> _shortModelInfos { get; set; } = new();

        public ObservableCollection<ShortModelInfo> ShortModelInfos
        {
            get { return _shortModelInfos; }
            set
            {
                _shortModelInfos = value;
                OnPropertyChanged(nameof(ShortModelInfos));
            }
        }

        public ShortModelInfo _modelsFilter { get; set; }

        //public List<string> _options { get; set; } = new();

        public ModelsModel()
        {
            _modelsFilter = new(string.Empty, string.Empty);
            AddAllModelInfos();
            
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }


        private void AddAllModelInfos()
        {
            ObservableCollection<ShortModelInfo> All = new();
            foreach (var configuration in ModelsModel._modelConfigurations.Values)
            {
                All.Add(new(configuration.Id, configuration.Name));
            }
            ShortModelInfos = new(All);
        }
    }
}
