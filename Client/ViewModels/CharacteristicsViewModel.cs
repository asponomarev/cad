using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
    public class CharacteristicsViewModel
    {
        public List<Characteristics> _characList { get; set; } = new()
            {
                new Characteristics(1, "charac_name", "int", "booksim"),
                new Characteristics(2, "charac_name2", "bool", "newxim"),
                new Characteristics(3, "charac_name3", "double", "topaz"),
                new Characteristics(4, "charac_name4", "int", "dec9"),
                new Characteristics(5, "charac_name5", "bool", "gpnocsim"),
                new Characteristics(6, "charac_name6", "double", "booksim"),
                new Characteristics(7, "charac_name7", "bool", "newxim"),
                new Characteristics(8, "charac_name8", "double", "topaz"),
                new Characteristics(9, "charac_name9", "int", "dec9"),
                new Characteristics(10, "charac_name10", "bool", "gpnocsim"),
                new Characteristics(11, "charac_name11", "int", "booksim"),
                new Characteristics(12, "charac_name12", "bool", "newxim"),
                new Characteristics(13, "charac_name13", "double", "topaz"),
                new Characteristics(14, "charac_name14", "int", "dec9"),
                new Characteristics(15, "charac_name15", "bool", "gpnocsim"),
            };

    }
}
