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
    public class ParametersModel : INotifyPropertyChanged
    {
        private string idFilter;
        private string nameFilter;
        private string valueTypeFilter;
        private string modelsFilter;
        private ObservableCollection<ShortParametersInfo> paramList;

        public ObservableCollection<ShortParametersInfo> AllParametersList { get; set; } = new()
        {
                new ShortParametersInfo("1", "param_name", "int", "Booksim"),
                new ShortParametersInfo("2", "param_name2", "bool", "newxim"),
                new ShortParametersInfo("3", "param_name3", "double", "topaz"),
                new ShortParametersInfo("4", "param_name4", "int", "dec9"),
                new ShortParametersInfo("5", "param_name5", "bool", "gpnocsim"),
                new ShortParametersInfo("6", "param_name6", "double", "booksim"),
                new ShortParametersInfo("7", "param_name7", "bool", "newxim"),
                new ShortParametersInfo("8", "param_name8", "double", "topaz"),
                new ShortParametersInfo("9", "param_name9", "int", "dec9"),
                new ShortParametersInfo("10", "param_name10", "bool", "gpnocsim"),
        };

        public ObservableCollection<ShortParametersInfo> ParamList
        {
            get { return paramList; }
            set
            {
                paramList = value;
                OnPropertyChanged(nameof(ParamList));
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

        public ParametersModel()
        {
            this.IdFilter = string.Empty;
            this.NameFilter = string.Empty;
            this.ValueTypeFilter = string.Empty;
            this.ModelsFilter = string.Empty;
            this.ParamList = new(AllParametersList);
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
