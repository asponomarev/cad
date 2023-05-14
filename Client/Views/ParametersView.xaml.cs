using Client.Views;
using Google.Protobuf.Compiler;
using System;
using System.Collections.Generic;
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
    public class ItemHandler
    {
        public ItemHandler()
        {
            Items = new List<Parameters>();
        }
        public List<Parameters> Items { get; private set; }
        public void Add(Parameters item)
        {
            Items.Add(item);
        }
    }
    public partial class ParametersView : UserControl
    {
        private readonly ItemHandler _itemHandler;
        public ParametersView()
        {
            InitializeComponent();
            _itemHandler = new ItemHandler();
            _itemHandler.Add(new Parameters(1, "param_name", "int", "booksim"));
            _itemHandler.Add(new Parameters(2, "param_name2", "bool", "newxim"));
            _itemHandler.Add(new Parameters(3, "param_name3", "double", "topaz"));
        }

        public List<Parameters> Items
        {
            get { return _itemHandler.Items; }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //listview.ItemSource = Items;

    }
}
