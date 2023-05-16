using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Client.Models
{
    public class Users : INotifyPropertyChanged
    {
        public string _nickname { get; set; }
        public string _email { get; set; }
        public string _name { get; set; }
        public string _surname { get; set; }
        public string _description { get; set; }
        public Users(string nickname, string name, string surname, string email, string description)
        {
            _nickname = nickname;
            _email = email;
            _name = name;
            _surname = surname;
            _description = description;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
