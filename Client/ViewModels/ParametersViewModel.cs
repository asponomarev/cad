using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UhlnocsClient;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
//using UhlnocsServer.Models.Properties.Characteristics;
//using UhlnocsServer.Models.Properties;
//using Client.Models.ForModelsModel;
using Grpc.Core;
using System.Text.Json;
//using UhlnocsServer.Models.Properties.Parameters;

namespace Client.ViewModels
{
    public class Parameters
    {
        public int id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string model { get; set; }
        public Parameters(int id, string name, string value, string model)
        {
            this.id = id;
            this.name = name;
            this.value = value;
            this.model = model;
        }
    }

    public class ParametersViewModel
    {
        List<Parameters> paramlist = new List<Parameters>()
         {
            new Parameters(1, "param_name", "int", "booksim"),
            new Parameters(2, "param_name2", "bool", "newxim"),
            new Parameters(3, "param_name3", "double", "topaz"),
            new Parameters(4, "param_name4", "int", "dec9"),
            new Parameters(5, "param_name5", "bool", "gpnocsim")
         };

    }
}
