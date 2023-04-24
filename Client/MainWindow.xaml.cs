using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UhlnocsClient;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        private readonly GrpcChannel _channel;
        private readonly ModelServiceProto.ModelServiceProtoClient _modelClient;
        private readonly CalculationServiceProto.CalculationServiceProtoClient _calculationClient;
        private readonly UserServiceProto.UserServiceProtoClient _userServiceClient;

        public MainWindow()
        {
            InitializeComponent();
            _configuration = CreateConfiguration();
            _channel = CreateGrpcChannel();
            _modelClient = new ModelServiceProto.ModelServiceProtoClient(_channel);
            _calculationClient = new CalculationServiceProto.CalculationServiceProtoClient(_channel);
            _userServiceClient = new UserServiceProto.UserServiceProtoClient(_channel);
        }

        private IConfiguration CreateConfiguration() 
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

        private GrpcChannel CreateGrpcChannel() 
            => GrpcChannel.ForAddress(_configuration.GetSection("ServerEndpoint").Value);

        private async Task<string> DummyMethod()
        {
            try
            {
                var result = await _modelClient.DeleteModelAsync(new ModelIdRequest() { ModelId = "testId" });
                return "Ok";
            }
            catch (Exception ex)
            {
                return ex.Message; 
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await DummyMethod();
            MyTextBlock.Text = result;
        }
    }
}
