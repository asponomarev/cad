using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class CharacteristicsModel : INotifyPropertyChanged
    {
        private string idFilter;
        private string nameFilter;
        private string valueTypeFilter;
        private string modelsFilter;
        private ObservableCollection<ShortCharacteristicInfo> characList;

        public ObservableCollection<ShortCharacteristicInfo> AllCharacteristicsList { get; set; } = new()
        {
                new ShortCharacteristicInfo("1", "charac_name", "int", "Booksim"),
                new ShortCharacteristicInfo("2", "charac_name2", "bool", "newxim"),
                new ShortCharacteristicInfo("3", "charac_name3", "double", "topaz"),
                new ShortCharacteristicInfo("4", "charac_name4", "int", "dec9"),
                new ShortCharacteristicInfo("5", "charac_name5", "bool", "gpnocsim"),
                new ShortCharacteristicInfo("6", "charac_name6", "double", "booksim"),
                new ShortCharacteristicInfo("7", "charac_name7", "bool", "newxim"),
                new ShortCharacteristicInfo("8", "charac_name8", "double", "topaz"),
                new ShortCharacteristicInfo("9", "charac_name9", "int", "dec9"),
                new ShortCharacteristicInfo("10", "charac_name10", "bool", "gpnocsim"),
        };

        public ObservableCollection<ShortCharacteristicInfo> CharacList
        {
            get { return characList; }
            set
            {
                characList = value;
                OnPropertyChanged(nameof(CharacList));
            }
        }

        public string IdFilter
        {
            get { return idFilter; }
            set
            {
                idFilter = value;
                OnPropertyChanged(nameof(IdFilter));
            }
        }
        public string NameFilter
        {
            get { return nameFilter; }
            set
            {
                nameFilter = value;
                OnPropertyChanged(nameof(NameFilter));
            }
        }
        public string ValueTypeFilter
        {
            get { return valueTypeFilter; }
            set
            {
                valueTypeFilter = value;
                OnPropertyChanged(nameof(ValueTypeFilter));
            }
        }
        public string ModelsFilter
        {
            get { return modelsFilter; }
            set
            {
                modelsFilter = value;
                OnPropertyChanged(nameof(ModelsFilter));
            }
        }

        public CharacteristicsModel()
        {
            this.IdFilter = string.Empty;
            this.NameFilter = string.Empty;
            this.ValueTypeFilter = string.Empty;
            this.ModelsFilter = string.Empty;
            this.CharacList = new(AllCharacteristicsList);
        }

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
