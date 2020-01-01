using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace wKpr.VM
{
    public class EntryVM : INotifyPropertyChanged
    {
        public EntryVM()
        {

        }

        #region Properties
        string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        string _user;
        public string User
        {
            get { return _user; }
            set
            {
                _user = value;
                RaisePropertyChanged();
            }
        }
        string _pass;
        public string Pass
        {
            get { return _pass; }
            set
            {
                _pass = value;
                RaisePropertyChanged();
            }
        }
        string _note;
        public string Note
        {
            get { return _note; }
            set
            {
                _note = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }
        #endregion
    }
}
