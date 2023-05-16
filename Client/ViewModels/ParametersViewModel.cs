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
using System.Windows.Controls;
using System.Windows;
//using UhlnocsServer.Models.Properties.Parameters;

namespace Client.ViewModels
{

    public class ParametersViewModel
    {
        //Parameters _param = new Parameters(1, "param_name", "int", "booksim");
        public List<Parameters> _paramList { get; set; } = new()
            {
                new Parameters(1, "param_name", "int", "booksim"),
                new Parameters(2, "param_name2", "bool", "newxim"),
                new Parameters(3, "param_name3", "double", "topaz"),
                new Parameters(4, "param_name4", "int", "dec9"),
                new Parameters(5, "param_name5", "bool", "gpnocsim"),
                new Parameters(6, "param_name6", "double", "booksim"),
                new Parameters(7, "param_name7", "bool", "newxim"),
                new Parameters(8, "param_name8", "double", "topaz"),
                new Parameters(9, "param_name9", "int", "dec9"),
                new Parameters(10, "param_name10", "bool", "gpnocsim"),
                new Parameters(11, "param_name11", "int", "booksim"),
                new Parameters(12, "param_name12", "bool", "newxim"),
                new Parameters(13, "param_name13", "double", "topaz"),
                new Parameters(14, "param_name14", "int", "dec9"),
                new Parameters(15, "param_name15", "bool", "gpnocsim"),
            };

    }
}
