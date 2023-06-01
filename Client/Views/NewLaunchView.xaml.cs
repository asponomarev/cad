using Client.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for NewLaunchView.xaml
    /// </summary>
    public partial class NewLaunchView : Window
    {
        public NewLaunchView()
        {
            InitializeComponent();
        }

        public class Characteristic
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        public class Parameter
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsSelected { get; set; } = false;
            public bool IsIteration { get; set; } = false;
            public double Value { get; set; }
        }

        // Для менюшки внутри рядов таблицы SelectedCharacteristics.
        public class MenuItemModel
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }
        }

        // Перемещает ряд из таблицы AllCharacteristics в SelectedCharacteristics.
        private void Characteristic_Checked(object sender, RoutedEventArgs e)
        {
            var selectedCharacteristic = (Characteristic)((CheckBox)e.OriginalSource).DataContext;
            if (selectedCharacteristic != null)
            {
                // Add the selected characteristic to the SelectedCharacteristics table
                var viewModel = (NewLaunchViewModel)DataContext;
                selectedCharacteristic.IsSelected = true;
                viewModel.SelectedCharacteristics.Add(selectedCharacteristic);
                viewModel.Characteristics.Remove(selectedCharacteristic);
            }
        }

        // Возвращает ряд из таблицы SelectedCharacteristics в AllCharacteristics.
        private void Characteristic_Unchecked(object sender, RoutedEventArgs e)
        {
            var unselectedCharacteristic = (Characteristic)((CheckBox)e.OriginalSource).DataContext;
            if (unselectedCharacteristic != null)
            {
                // Remove the unselected characteristic from the SelectedCharacteristics table
                var viewModel = (NewLaunchViewModel)DataContext;
                unselectedCharacteristic.IsSelected = false;
                viewModel.Characteristics.Add(unselectedCharacteristic);
                viewModel.SelectedCharacteristics.Remove(unselectedCharacteristic);
            }
        }


        // Перемещает ряд из таблицы AllParameters в SelectedParameters.
        private void Parameter_Checked(object sender, RoutedEventArgs e)
        {
            var selectedParameter = (Parameter)((CheckBox)e.OriginalSource).DataContext;
            if (selectedParameter != null)
            {
                // Add the selected Parameter to the SelectedParameters table
                var viewModel = (NewLaunchViewModel)DataContext;
                selectedParameter.IsSelected = true;
                viewModel.SelectedParameters.Add(selectedParameter);
                viewModel.Parameters.Remove(selectedParameter);
            }
        }

        // Возвращает ряд из таблицы SelectedParameters в AllParameters.
        private void Parameter_Unchecked(object sender, RoutedEventArgs e)
        {
            var unselectedParameter = (Parameter)((CheckBox)e.OriginalSource).DataContext;
            if (unselectedParameter != null)
            {
                // Remove the unselected Parameter from the SelectedParameters table
                var viewModel = (NewLaunchViewModel)DataContext;
                unselectedParameter.IsSelected = false;
                viewModel.Parameters.Add(unselectedParameter);
                viewModel.SelectedParameters.Remove(unselectedParameter);
            }
        }

        // Скрывает и показывает нужные поля ввода, в зависимости от выбранного Optimization Algorithm.
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ConstantStep.IsChecked == true)
            {
                StepValue.Visibility = Visibility.Visible;
                NumOfIters.Visibility = Visibility.Visible;

                MaxFreqOfDataGen.Visibility = Visibility.Collapsed;
                MaxNumOfIters.Visibility = Visibility.Collapsed;
                Accuracy.Visibility = Visibility.Collapsed;
            }
            else if (SmartConstantStep.IsChecked == true)
            {
                StepValue.Visibility = Visibility.Visible;
                NumOfIters.Visibility = Visibility.Visible;
                Accuracy.Visibility = Visibility.Visible;
                MaxNumOfIters.Visibility = Visibility.Visible;

                MaxFreqOfDataGen.Visibility = Visibility.Collapsed;
            }
            else if (BinarySearch.IsChecked == true || GoldenSection.IsChecked == true)
            {
                MaxFreqOfDataGen.Visibility = Visibility.Visible;
                NumOfIters.Visibility = Visibility.Visible;
                Accuracy.Visibility = Visibility.Visible;

                StepValue.Visibility = Visibility.Collapsed;
                MaxNumOfIters.Visibility = Visibility.Collapsed;
            }
            else if (SmartBinarySearch.IsChecked == true || SmartGoldenSection.IsChecked == true)
            {
                MaxFreqOfDataGen.Visibility = Visibility.Visible;
                MaxNumOfIters.Visibility = Visibility.Visible;
                Accuracy.Visibility = Visibility.Visible;

                StepValue.Visibility = Visibility.Collapsed;
                NumOfIters.Visibility = Visibility.Collapsed;
            }
        }

        // Скрывает все поля ввода, если ни один Optimization Algorithm не выбран.
        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ConstantStep.IsChecked == false && SmartConstantStep.IsChecked == false &&
                BinarySearch.IsChecked == false && SmartBinarySearch.IsChecked == false &&
                GoldenSection.IsChecked == false && SmartGoldenSection.IsChecked == false)
            {
                StepValue.Visibility = Visibility.Collapsed;
                MaxFreqOfDataGen.Visibility = Visibility.Collapsed;
                NumOfIters.Visibility = Visibility.Collapsed;
                MaxNumOfIters.Visibility = Visibility.Collapsed;
                Accuracy.Visibility = Visibility.Collapsed;
            }
        }

        // Предотвращает ввод данных отличных от int и double в поля ввода относящиеся к Optimization Algorithm и Decision Accuracy.
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9.-]+"); // Regular expression to match non-numeric values
            e.Handled = regex.IsMatch(e.Text);
        }

        // Event Handler для кнопки Help.
        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Some help.");
        }

        // Event Handler для кнопки Run.
        private void Run_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Running...");
        }
    }
}
