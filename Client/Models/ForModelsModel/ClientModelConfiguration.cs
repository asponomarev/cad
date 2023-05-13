using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.ForModelsModel
{
    public class ClientModelConfiguration
    {
        public string _id { get; set; }

        public string _name { get; set; }

        public string _description { get; set; }

        public string _modelFilePath { get; set; }

        public int _modelOkExitCode { get; set; }

        public string _modelArgumentsFormatString { get; set; }

        public string _preparerFilePath { get; set; }

        public int _preparerOkExitCode { get; set; }

        public string _collectorFilePath { get; set; }

        public int _collectorOkExitCode { get; set; }

        public bool _collectFromStdout { get; set; }

        public double _performance { get; set; }

        public string _responsibleUsers { get; set; }

        public List<ClientParameterInfo> _parameters { get; set; }

        public List<ClientCharacteristicInfo> _characteristics { get; set; }

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
                                  List<ClientParameterInfo> parameters,
                                  List<ClientCharacteristicInfo> characteristics)
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
            _characteristics = new();
            _parameters = new();
        }
    }
}
