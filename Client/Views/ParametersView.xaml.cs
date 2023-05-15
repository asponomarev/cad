using Client.Views;
using Google.Protobuf.Compiler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ParametersView.xaml
    /// </summary>
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
    
    public partial class ParametersView : UserControl
    {
        public ParametersView()
        {
            InitializeComponent();
            List<Parameters> paramlist = new List<Parameters>()
            {
                new Parameters(1, "param_name", "int", "booksim"),
                new Parameters(2, "param_name2", "bool", "newxim"),
                new Parameters(3, "param_name3", "double", "topaz"),
                new Parameters(4, "param_name4", "int", "dec9"),
                new Parameters(5, "param_name5", "bool", "gpnocsim")
            };
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //listview.ItemSource = Items;

    }
}
