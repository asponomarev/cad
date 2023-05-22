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

namespace Client.ViewModels
{
    public class ModelsViewModel
    {
        public ModelsModel _modelsModel { get; set; }
        public ICommand GetCharacteristicsCommand { get; private set; }
        private readonly ModelServiceProto.ModelServiceProtoClient _modelClient;

        public ModelsViewModel(ModelServiceProto.ModelServiceProtoClient modelClient)
        {
            _modelClient = modelClient;
            _modelsModel = new ModelsModel();
            GetCharacteristicsCommand = new RelayCommand(GetCharacteristicsMethod);
            GetCharacteristicsMethod();
            InitializeDictionaries();
            //ClientCharacteristicsGrid.ItemsSource = _modelsModel.Characteristics;
        }

        private async void InitializeDictionaries()
        {
            try
            {
                Metadata requestHeader = new();
                requestHeader.Add("user", "id");

                var parametersReply = await _modelClient.GetParametersWithModelsAsync(new ModelEmptyMessage(), requestHeader);
                ModelsModel._parametersWithModels = JsonSerializer.Deserialize<Dictionary<string, ParameterWithModels>> (parametersReply.ParametersWithModelsJson, MainWindow.SerializerOptions);
                _modelsModel._modelConfiguration._characteristics.Add(new ClientCharacteristicInfo("123!!!!!!!!", "name", "dsdf", PropertyValueType.Int));
            }
            catch (Exception ex)
            {
                _modelsModel._modelConfiguration._characteristics.Add(new ClientCharacteristicInfo("123------", "name", "dsdf", PropertyValueType.Int));
            }
        }

        private void GetCharacteristicsMethod()
        {
            _modelsModel._options.Add("true");
            _modelsModel._options.Add("false");
            _modelsModel._modelConfiguration._characteristics.Add(new ClientCharacteristicInfo("123", "name", "dsdf", PropertyValueType.Int));
            _modelsModel._modelConfiguration._characteristics.Add(new ClientCharacteristicInfo("1jh23", "name", "dsdf", PropertyValueType.Int));
            /*try
            {
                var result = await _modelClient.DeleteModelAsync(new ModelIdRequest() { ModelId = "testId" });
                Dummy.RequetResult = "Ok";
            }
            catch (Exception ex)
            {
                Dummy.RequetResult = ex.Message;
            }*/
        }



    }
}
