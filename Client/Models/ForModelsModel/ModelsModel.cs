using Client.Models.ForModelsModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models
{
    public class ModelsModel : INotifyPropertyChanged
    {
        public List<ClientCharacteristicInfo> Characteristics { get; set; } = new();



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
