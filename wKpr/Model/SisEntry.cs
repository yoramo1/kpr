using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace wKpr.Model
{
    public enum EntryState {  New, Loaded }
    [DebuggerDisplay("SisEntry  {Name}, {Pass}")]
    public class SisEntry : INotifyPropertyChanged
    {
        public SisEntry()
        {
            State = EntryState.New;
        }

        internal SisEntry(SisEntry copyFrom)
        {
            State = EntryState.New;
            Name = copyFrom.Name;
            User = copyFrom.User;
            Url = copyFrom.Url;
            Pass = copyFrom.Pass;
            Note = copyFrom.Note;
            OldPass = copyFrom.OldPass;
        }
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string User { get; set; }

        [XmlAttribute]
        public string Pass { get; set; }

        [XmlAttribute]
        public string Note { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

        internal bool ReadyToApply()
        {
            return !string.IsNullOrEmpty(Name);
        }

        [XmlAttribute]
        public string[] OldPass { get; set; }

        [XmlAttribute]
        public DateTime LastUpdated { get; set; }

        [XmlIgnore]
        public EntryState State { get; set; }


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
