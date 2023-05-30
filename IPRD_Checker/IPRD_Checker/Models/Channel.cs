using System;
using System.ComponentModel;
using System.Diagnostics;
using static System.String;

namespace IPRD_Checker.Models
{
    public class Channel : INotifyPropertyChanged
    {
        [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged("URL");
            }
        }
        
        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public Channel()
        {
            Url = Empty;
            Name = Empty;
            Status = Status.Unchecked;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public override bool Equals(object obj)
        {
            var flag = obj == null;
            bool flag2;
            if (flag)
            {
                flag2 = false;
            }
            else
            {
                var channel = obj as Channel;
                var flag3 = channel == null;
                if (flag3)
                {
                    flag2 = false;
                }
                else
                {
                    var flag4 = string.Equals(Url, channel.Url, StringComparison.CurrentCultureIgnoreCase);
                    flag2 = flag4;
                }
            }
            return flag2;
        }
        
        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }
        
        private string _name;
        
        private Status _status;
        
        private string _url;
    }
}
