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

    public class ParametersViewModel
    {
        private readonly ModelServiceProto.ModelServiceProtoClient ModelClient;

        public ICommand FilterParametersCommand { get; private set; }
        public ParametersModel ParametersModel { get; private set; } = new ParametersModel();

        public ParametersViewModel(ModelServiceProto.ModelServiceProtoClient modelClient)
        {
            ModelClient = modelClient;
            FilterParametersCommand = new RelayCommand(FilterParameters);
        }

        private async void FilterParameters()
        {
            ObservableCollection<ShortParametersInfo> Filtered = new();
            foreach (var info in ParametersModel.AllParametersList)
            {
                if (info.id.Contains(ParametersModel.IdFilter) && !string.IsNullOrEmpty(ParametersModel.IdFilter) ||
                    info.name.Contains(ParametersModel.NameFilter) && !string.IsNullOrEmpty(ParametersModel.NameFilter) ||
                    info.valueType == ParametersModel.ValueTypeFilter && !string.IsNullOrEmpty(ParametersModel.ValueTypeFilter) ||
                    info.models.Contains(ParametersModel.ModelsFilter) && !string.IsNullOrEmpty(ParametersModel.ModelsFilter))
                {
                    Filtered.Add(info);
                }
            }
            ParametersModel.ParamList = new(Filtered);

        }
    }
}
