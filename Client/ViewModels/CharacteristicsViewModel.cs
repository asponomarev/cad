using Client.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UhlnocsClient;

namespace Client.ViewModels
{
    public class CharacteristicsViewModel
    {
        private readonly ModelServiceProto.ModelServiceProtoClient ModelClient;

        public ICommand FilterCharacteristicsCommand { get; private set; }
        public CharacteristicsModel CharacteristicsModel { get; private set; } = new CharacteristicsModel();

        public CharacteristicsViewModel(ModelServiceProto.ModelServiceProtoClient modelClient)
        {
            ModelClient = modelClient;
            FilterCharacteristicsCommand = new RelayCommand(FilterCharacteristics);
        }

        private async void FilterCharacteristics()
        {
            ObservableCollection<ShortCharacteristicInfo> Filtered = new();
            foreach(var info in CharacteristicsModel.AllCharacteristicsList)
            {
                if (info.id.Contains(CharacteristicsModel.IdFilter) && !string.IsNullOrEmpty(CharacteristicsModel.IdFilter) || 
                    info.name.Contains(CharacteristicsModel.NameFilter) && !string.IsNullOrEmpty(CharacteristicsModel.NameFilter) ||
                    info.valueType == CharacteristicsModel.ValueTypeFilter && !string.IsNullOrEmpty(CharacteristicsModel.ValueTypeFilter) ||
                    info.models.Contains(CharacteristicsModel.ModelsFilter) && !string.IsNullOrEmpty(CharacteristicsModel.ModelsFilter))
                {
                    Filtered.Add(info);
                }
            }
            CharacteristicsModel.CharacList = new(Filtered);

        }



    }
}
