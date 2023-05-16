using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using static Client.Views.NewLaunchView;

namespace Client.ViewModels
{
    public class NewLaunchViewModel
    {
        public ObservableCollection<Characteristic> Characteristics { get; set; }
        public ObservableCollection<Characteristic> SelectedCharacteristics { get; set; }
        public ObservableCollection<MenuItemModel> MenuModels { get; set; }

        public ObservableCollection<Parameter> Parameters { get; set; }
        public ObservableCollection<Parameter> SelectedParameters { get; set; }

        // Decision Accuracy Value.
        public double Value { get; set; }

        // Инициализация данных для таблиц.
        public NewLaunchViewModel()
        {
            Characteristics = new ObservableCollection<Characteristic>()
            {
                new Characteristic { Id = 1, Name = "Characteristic 1" },
                new Characteristic { Id = 2, Name = "Characteristic 2" },
                new Characteristic { Id = 3, Name = "Characteristic 3" },
                new Characteristic { Id = 4, Name = "Characteristic 4" },
                new Characteristic { Id = 1, Name = "Characteristic 1" },
                new Characteristic { Id = 2, Name = "Characteristic 2" },
                new Characteristic { Id = 3, Name = "Characteristic 3" },
                new Characteristic { Id = 4, Name = "Characteristic 4" },
                new Characteristic { Id = 1, Name = "Characteristic 1" },
                new Characteristic { Id = 2, Name = "Characteristic 2" },
                new Characteristic { Id = 3, Name = "Characteristic 3" },
                new Characteristic { Id = 4, Name = "Characteristic 4" },
                new Characteristic { Id = 1, Name = "Characteristic 1" },
                new Characteristic { Id = 2, Name = "Characteristic 2" },
                new Characteristic { Id = 3, Name = "Characteristic 3" },
                new Characteristic { Id = 4, Name = "Characteristic 4" },
                new Characteristic { Id = 1, Name = "Characteristic 1" },
                new Characteristic { Id = 2, Name = "Characteristic 2" },
                new Characteristic { Id = 3, Name = "Characteristic 3" },
                new Characteristic { Id = 4, Name = "Characteristic 4" },
                new Characteristic { Id = 1, Name = "Characteristic 1" },
                new Characteristic { Id = 2, Name = "Characteristic 2" },
                new Characteristic { Id = 3, Name = "Characteristic 3" },
                new Characteristic { Id = 4, Name = "Characteristic 4" },
            };
            SelectedCharacteristics = new ObservableCollection<Characteristic>();

            MenuModels = new ObservableCollection<MenuItemModel>
            {
                new MenuItemModel() { Name = "Model 1", IsChecked = false },
                new MenuItemModel() { Name = "Model 2", IsChecked = false },
                new MenuItemModel() { Name = "Model 3", IsChecked = false },
            };

            Parameters = new ObservableCollection<Parameter>()
            {
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
                new Parameter { Id = 1, Name = "Parameter 1" },
                new Parameter { Id = 2, Name = "Parameter 2" },
                new Parameter { Id = 3, Name = "Parameter 3" },
                new Parameter { Id = 4, Name = "Parameter 4" },
                new Parameter { Id = 5, Name = "Parameter 5" },
            };
            SelectedParameters = new ObservableCollection<Parameter>();
        }
    }

    // Предотвращает ввод данных отличных от int и double в колонку Value в таблице SelectedParameterss.
    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? "").ToString()!;

            if (!Regex.IsMatch(input, @"^(-)?\d+(\.\d+)?$"))
            {
                return new ValidationResult(false, "Please enter a numeric value.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
