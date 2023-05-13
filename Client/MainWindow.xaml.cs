using Client.ViewModels;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.PortableExecutable;
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
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Width = screenWidth;
            this.Height = screenHeight;

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

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new UserViewModel(_userServiceClient, _modelClient);
        }

        private void Parameters_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new ParametersViewModel();
        }

        private void Characteristics_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new CharacteristicsViewModel();
        }
    }
}
