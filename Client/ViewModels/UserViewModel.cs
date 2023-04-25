using Client.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using UhlnocsClient;

namespace Client.ViewModels
{
    public class UserViewModel
    {
        private readonly UserServiceProto.UserServiceProtoClient _userServiceClient;
        private readonly ModelServiceProto.ModelServiceProtoClient _modelClient;

        public UserViewModel(UserServiceProto.UserServiceProtoClient userServiceClient, ModelServiceProto.ModelServiceProtoClient modelClient)
        {
            _userServiceClient = userServiceClient;
            _modelClient = modelClient;
            DummyCommand = new RelayCommand(DummyMethod);
        }

        public ICommand DummyCommand { get; private set; }
        public DummyModel Dummy { get; private set; } = new DummyModel();

        private async void DummyMethod()
        {
            try
            {
                var result = await _modelClient.DeleteModelAsync(new ModelIdRequest() { ModelId = "testId" });
                Dummy.RequetResult = "Ok";
            }
            catch (Exception ex)
            {
                Dummy.RequetResult = ex.Message;
            }
        }
    }
}
