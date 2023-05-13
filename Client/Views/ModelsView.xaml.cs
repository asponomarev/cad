using Client.ViewModels;
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
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class ModelsView : UserControl
    {
        //public ObservableCollection<Order> Orders { get; set; } = new ObservableCollection<Order>();
        /*public List<Order> Orders { get; set; } = new List<Order>(); //viewmodel*/

        public ModelsView()
        {
            InitializeComponent();
            /*DataContext = new ViewModels.ModelsViewModel();*/
            //ClientCharacteristicsGrid.ItemsSource = _modelsModel.Characteristics;

            /*Orders.Add(new Order { OrderNumber = 1, CustomerName = "John Doe", OrderDate = DateTime.Today });
            Orders.Add(new Order { OrderNumber = 2, CustomerName = "Jane Smith", OrderDate = DateTime.Today.AddDays(1) });
            Orders.Add(new Order { OrderNumber = 3, CustomerName = null, OrderDate = DateTime.Today.AddDays(1) });

            // Set the ItemsSource property of the DataGrid
            ordersGrid.ItemsSource = Orders;*/

        }

        //models
        /*public class Order
        {
            public int OrderNumber { get; set; }
            public string? CustomerName { get; set; }
            public DateTime OrderDate { get; set; }
        }*/
    }
}
