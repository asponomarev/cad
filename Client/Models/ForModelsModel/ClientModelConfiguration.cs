using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UhlnocsServer.Models;
using UhlnocsServer.Models.Properties.Characteristics;
using UhlnocsServer.Models.Properties.Parameters;

namespace Client.Models.ForModelsModel
{
    public class ClientModelConfiguration
    {
        private string _id { get; set; }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _name { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }


        private string _description { get; set; }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }


        private string _modelFilePath { get; set; }

        public string ModelFilePath
        {
            get { return _modelFilePath; }
            set
            {
                _modelFilePath = value;
                OnPropertyChanged(nameof(ModelFilePath));
            }
        }

        private int _modelOkExitCode { get; set; }

        public int ModelOkExitCode
        {
            get { return _modelOkExitCode; }
            set
            {
                _modelOkExitCode = value;
                OnPropertyChanged(nameof(ModelOkExitCode));
            }
        }

        private string _modelArgumentsFormatString { get; set; }

        public string ModelArgumentsFormatString
        {
            get { return _modelArgumentsFormatString; }
            set
            {
                _modelArgumentsFormatString = value;
                OnPropertyChanged(nameof(ModelArgumentsFormatString));
            }
        }

        private string _preparerFilePath { get; set; }

        public string PreparerFilePath
        {
            get { return _preparerFilePath; }
            set
            {
                _preparerFilePath = value;
                OnPropertyChanged(nameof(PreparerFilePath));
            }
        }

        private int _preparerOkExitCode { get; set; }

        public int PreparerOkExitCode
        {
            get { return _preparerOkExitCode; }
            set
            {
                _preparerOkExitCode = value;
                OnPropertyChanged(nameof(PreparerOkExitCode));
            }
        }

        private string _collectorFilePath { get; set; }

        public string CollectorFilePath
        {
            get { return _collectorFilePath; }
            set
            {
                _collectorFilePath = value;
                OnPropertyChanged(nameof(CollectorFilePath));
            }
        }

        private int _collectorOkExitCode { get; set; }

        public int CollectorOkExitCode
        {
            get { return _collectorOkExitCode; }
            set
            {
                _collectorOkExitCode = value;
                OnPropertyChanged(nameof(CollectorOkExitCode));
            }
        }

        private bool _collectFromStdout { get; set; }

        public bool CollectFromStdout
        {
            get { return _collectFromStdout; }
            set
            {
                _collectFromStdout = value;
                OnPropertyChanged(nameof(CollectFromStdout));
            }
        }

        private double _performance { get; set; }

        public double Perfomance
        {
            get { return _performance; }
            set
            {
                _performance = value;
                OnPropertyChanged(nameof(Perfomance));
            }
        }

        private string _responsibleUsers { get; set; }

        public string ResponsibleUsers
        {
            get { return _responsibleUsers; }
            set
            {
                _responsibleUsers = value;
                OnPropertyChanged(nameof(ResponsibleUsers));
            }
        }

        private ObservableCollection<ClientParameterInfo> _parameters { get; set; }

        public ObservableCollection<ClientParameterInfo> Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                OnPropertyChanged(nameof(Parameters));
            }
        }

        private ObservableCollection<ClientCharacteristicInfo> _characteristics { get; set; }

        public ObservableCollection<ClientCharacteristicInfo> Characteristics
        {
            get { return _characteristics; }
            set
            {
                _characteristics = value;
                OnPropertyChanged(nameof(Characteristics));
            }
        }

        public ClientModelConfiguration(string id,
                                  string name,
                                  string description,
                                  string modelFilePath,
                                  int modelOkExitCode,
                                  string modelArgumentsFormatString,
                                  string preparerFilePath,
                                  int preparerOkExitCode,
                                  string collectorFilePath,
                                  int collectorOkExitCode,
                                  bool collectFromStdout,
                                  double performance,
                                  string responsibleUsers,
                                  ObservableCollection<ClientParameterInfo> parameters,
                                  ObservableCollection<ClientCharacteristicInfo> characteristics)
        {
            _id = id;
            _name = name;
            _description = description;

            _modelFilePath = modelFilePath;
            _modelOkExitCode = modelOkExitCode;
            _modelArgumentsFormatString = modelArgumentsFormatString;

            _preparerFilePath = preparerFilePath;
            _preparerOkExitCode = preparerOkExitCode;

            _collectorFilePath = collectorFilePath;
            _collectorOkExitCode = collectorOkExitCode;
            _collectFromStdout = collectFromStdout;
            _performance = performance;

            _responsibleUsers = responsibleUsers;
            _parameters = parameters;
            _characteristics = characteristics;
        }

        public ClientModelConfiguration()
        {
            Characteristics = new();
            Parameters = new();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public static ClientModelConfiguration FromServerModelConfiguration(ModelConfiguration serverInfo, double performance)
        {
            ObservableCollection<ClientParameterInfo> parameters = new();
            ObservableCollection<ClientCharacteristicInfo> characteristics = new();

            foreach (var serverParameter in serverInfo.ParametersInfo)
            {
                parameters.Add(ClientParameterInfo.FromServerParameterInfo(serverParameter));
            }

            foreach (var serverCharacteristic in serverInfo.CharacteristicsInfo)
            {
                characteristics.Add(ClientCharacteristicInfo.FromServerCharacteristicInfo(serverCharacteristic));
            }

            return new(serverInfo.Id, serverInfo.Name, serverInfo.Description, serverInfo.ModelFilePath, serverInfo.ModelOkExitCode,
                       serverInfo.ModelArgumentsFormatString, serverInfo.PreparerFilePath, serverInfo.PreparerOkExitCode,
                       serverInfo.CollectorFilePath, serverInfo.CollectorOkExitCode, serverInfo.CollectFromStdout, performance,
                       string.Join(", ", serverInfo.ResponsibleUsers), parameters, characteristics);
        }

        public static ModelConfiguration ToServerModelConfiguration(ClientModelConfiguration clientInfo)
        {
            List<ParameterInfo> parameters = new();
            List<CharacteristicInfo> characteristics = new();

            foreach (var clientParameter in clientInfo.Parameters)
            {
                parameters.Add(ClientParameterInfo.ToServerParameterInfo(clientParameter));
            }

            foreach (var clientCharacteristic in clientInfo.Characteristics)
            {
                characteristics.Add(ClientCharacteristicInfo.ToServerCharacteristicInfo(clientCharacteristic));
            }

            return new(clientInfo.Id, clientInfo.Name, clientInfo.Description, clientInfo.ModelFilePath, clientInfo.ModelOkExitCode,
                       clientInfo.ModelArgumentsFormatString, clientInfo.PreparerFilePath, clientInfo.PreparerOkExitCode,
                       clientInfo.CollectorFilePath, clientInfo.CollectorOkExitCode, clientInfo.CollectFromStdout,
                       new List<string>(clientInfo.ResponsibleUsers.Split(", ")), parameters, characteristics);
        }
    }
}
