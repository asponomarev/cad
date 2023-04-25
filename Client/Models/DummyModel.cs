using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class DummyModel : INotifyPropertyChanged
    {
        private string _result;

        public string RequetResult
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged(nameof(RequetResult));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
