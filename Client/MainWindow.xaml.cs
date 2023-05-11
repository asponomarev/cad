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
        private readonly ModelsViewModel _modelsViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Get screen size
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            // Set window size as a percentage of screen size
            this.Width = screenWidth;
            this.Height = screenHeight; // Set aspect ratio as needed

            _configuration = CreateConfiguration();
            _channel = CreateGrpcChannel();
            _modelClient = new ModelServiceProto.ModelServiceProtoClient(_channel);
            _calculationClient = new CalculationServiceProto.CalculationServiceProtoClient(_channel);
            _userServiceClient = new UserServiceProto.UserServiceProtoClient(_channel);
            _modelsViewModel = new ModelsViewModel(_modelClient);
        }

        private IConfiguration CreateConfiguration() 
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

        private GrpcChannel CreateGrpcChannel() 
            => GrpcChannel.ForAddress(_configuration.GetSection("ServerEndpoint").Value);

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new UserViewModel(_userServiceClient, _modelClient);
        }

        private void ParametersButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new ParametersViewModel();
        }

        private void CharacteristicsButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new CharacteristicsViewModel();
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = _modelsViewModel;
        }
    }
}
