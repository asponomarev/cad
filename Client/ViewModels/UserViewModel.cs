using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
    public class UserViewModel
    {
        public List<Users> _characList { get; set; } = new()
        {
            new Users("qwerty1", "name1", "surname1", "email1", "description"),
            new Users("qwerty2", "name2", "surname2", "email2", "description"),
            new Users("qwerty3", "name3", "surname3", "email3", "description"),
            new Users("qwerty4", "name4", "surname4", "email4", "description"),
            new Users("qwerty5", "name5", "surname5", "email5", "description"),
        };

    }
}
