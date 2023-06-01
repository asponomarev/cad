using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UhlnocsClient;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties;
using Client.Models.ForModelsModel;
using Grpc.Core;
using System.Text.Json;
using UhlnocsServer.Models.Properties.Parameters;
using System.Collections.ObjectModel;
using UhlnocsServer.Models;

namespace Client.ViewModels
{
    public class ModelsViewModel
    {
        public ModelsModel _modelsModel { get; set; }
        public ICommand AddCharacteristicsCommand { get; private set; }
        public ICommand AddParametersCommand { get; private set; }
        public ICommand FilterModelsCommand { get; private set; }

        private readonly ModelServiceProto.ModelServiceProtoClient _modelClient;

        public ModelsViewModel(ModelServiceProto.ModelServiceProtoClient modelClient)
        {
            _modelClient = modelClient;
            InitializeDictionaries();
            _modelsModel = new ModelsModel();
            AddCharacteristicsCommand = new RelayCommand(AddCharacteristicsMethod);
            AddParametersCommand = new RelayCommand(AddParametersMethod);
            FilterModelsCommand = new RelayCommand(FilterModelsMethod);
        }

        private void FilterModelsMethod()
        {
            ObservableCollection<ShortModelInfo> Filtered = new();
            foreach (var configuration in ModelsModel._modelConfigurations.Values)
            {
                if (configuration.Id.Contains(_modelsModel._modelsFilter.Id) && !string.IsNullOrEmpty(_modelsModel._modelsFilter.Id) ||
                    configuration.Name.Contains(_modelsModel._modelsFilter.Name) && !string.IsNullOrEmpty(_modelsModel._modelsFilter.Name) ||
                    string.IsNullOrEmpty(_modelsModel._modelsFilter.Id) && string.IsNullOrEmpty(_modelsModel._modelsFilter.Name))
                {
                    Filtered.Add(new(configuration.Id, configuration.Name));
                }
            }
            _modelsModel.ShortModelInfos = new(Filtered);
        }

        private void AddCharacteristicsMethod()
        {
            _modelsModel._modelConfiguration.Characteristics.Add(new(null, null, null, null)) ;
        }

        private void AddParametersMethod()
        {
            _modelsModel._modelConfiguration.Parameters.Add(new(null, null, null, null, null, null, null, null));
        }

        private async void InitializeDictionaries()
        {
            try
            {
                Metadata requestHeader = new();
                requestHeader.Add("user", "id");

                var parametersReply = await _modelClient.GetParametersWithModelsAsync(new ModelEmptyMessage(), requestHeader);
                ModelsModel._parametersWithModels = JsonSerializer.Deserialize<Dictionary<string, ParameterWithModels>>(parametersReply.ParametersWithModelsJson, MainWindow.SerializerOptions);
                
                var characteristicsReply = await _modelClient.GetCharacteristicsWithModelsAsync(new ModelEmptyMessage(), requestHeader);
                ModelsModel._characteristicsWithModels = JsonSerializer.Deserialize<Dictionary<string, CharacteristicWithModels>>(characteristicsReply.CharacteristicsWithModelsJson, MainWindow.SerializerOptions);
                
                var modelsReply = await _modelClient.GetModelsConfigurationsAsync(new ModelEmptyMessage(), requestHeader);
                foreach(var modelReply in modelsReply.ModelsConfigurationsReplies)
                {
                    JsonDocument document = JsonDocument.Parse(modelReply.ModelConfigurationJson);
                    ModelConfiguration serverConfiguration = ModelConfiguration.FromJsonDocument(document);
                    ClientModelConfiguration clientConfiguration = ClientModelConfiguration.FromServerModelConfiguration(serverConfiguration, modelReply.Performance);
                    ModelsModel._modelConfigurations.Add(clientConfiguration.Id, clientConfiguration);
                }
                
            }
            catch (Exception ex)
            {
                // Make notification
            }
        }
    }
}
