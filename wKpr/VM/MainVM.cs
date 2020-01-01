using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using wKpr.Model;
using wKpr.Properties;
using wKpr.Utils;
using static wKpr.Model.Kpr;

namespace wKpr.VM
{
    public class MainVM : INotifyPropertyChanged
    {
        private static readonly NLog.Logger _sLog = NLog.LogManager.GetCurrentClassLogger();
        public MainVM()
        {
            _timer = new Timer(Settings.Default.Timeout);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
            SelectedEntry = new SisEntry();
            NotInEditMode = true;
            ActivatEditUI(false, false);
            InitCommands();
        }



        #region Command
        private void InitCommands()
        {
            cmdNew = new DelegateCommand(OnNew, CanNew);
            cmdAdd = new DelegateCommand<object>(OnAdd, canAdd);
            cmdUpdate = new DelegateCommand(OnUpdate, CanUpdate);
            cmdDelete = new DelegateCommand(OnDelete, CanDelete);
            cmdList = new DelegateCommand(OnList, CanList);
            cmdAcceptAutentication = new DelegateCommand<object>(OnAcceptAutentication, CanAcceptAutentication);
            cmdBrouse = new DelegateCommand(OnBrowse, CanBrowse);
            cmdEdit = new DelegateCommand(OnEdit, CanEdit);
        }
        public DelegateCommand cmdNew { get; private set; }
        public DelegateCommand<object> cmdAdd { get; private set; }
        public DelegateCommand cmdUpdate { get; private set; }
        public DelegateCommand cmdDelete { get; private set; }
        public DelegateCommand cmdList { get; private set; }
        public DelegateCommand<object> cmdAcceptAutentication { get; private set; }
        public DelegateCommand cmdBrouse { get; private set; }
        public DelegateCommand cmdEdit { get; private set; }

        private void OnNew()
        {
            SelectedEntry = new SisEntry();
            ActivatEditUI(true, true);
        }
        private bool CanNew()
        {
            return true;
        }
        private void OnAdd(object ctl)
        {
            try
            {
                if (Kpr.Add(SelectedEntry, PassLst) && _decoder != null)
                {
                    Kpr.Save(PassLst, FilePath, _decoder.convertToUNSecureString());
                    SelectedEntry = new SisEntry();
                    SetMessage("Entry added", MessageType.Message);
                    ActivatEditUI(false, false);
                }
            }
            catch (Exception)
            {
                SetMessage("fail to add entry", MessageType.Error);
            }

        }
        private bool canAdd(object ctl)
        {
            return SelectedEntry != null && SelectedEntry.State == EntryState.New;
        }
        private void OnUpdate()
        {
            _sLog.Debug($"Update");
            if (_editEntry != null && SelectedEntry != null)
            {
                try
                {
                    Kpr.UpdateEntry(_editEntry, SelectedEntry, PassLst);

                    Kpr.Save(PassLst, FilePath, _decoder.convertToUNSecureString());
                    SetEditMode(false);
                    SetMessage("update OK", MessageType.Message);
                }
                catch (Exception err)
                {
                    _sLog.Error(err,$"Exception");
                    SetMessage("fail to update", MessageType.Error);
                }
                ActivatEditUI(false, false);
            }
        }
        private bool CanUpdate()
        {
            return !NotInEditMode;
        }
        private void OnDelete()
        {
            _sLog.Debug($"Delete - '{SelectedEntry?.Name}'");
            Kpr.Remove(PassLst, SelectedEntry);
            Kpr.Save(PassLst, FilePath, _decoder.convertToUNSecureString());
            OnList();
        }
        private bool CanDelete()
        {
            return SelectedEntry != null && NotInEditMode;
        }
        private void OnList()
        {
            try
            {
                _sLog.Debug($"OnList filter={FilterPattern}");

                List<SisEntry> items = new List<SisEntry>();
                if (PassLst != null)
                {
                    foreach (SisEntry aa in PassLst.Items)
                    {
                        if (!string.IsNullOrEmpty(FilterPattern))
                        {
                            if (aa.Name.Contains(FilterPattern, StringComparison.OrdinalIgnoreCase))
                                items.Add(aa);
                        }
                        else
                            items.Add(aa);
                    }
                    ListCollectionView _customerView = CollectionViewSource.GetDefaultView(items) as ListCollectionView;
                    _customerView.CustomSort = new ItemSorter();

                    ItemList = _customerView;
                    RaisePropertyChanged("ItemList");
                    SetMessage("OnList OK", MessageType.Message);
                }
            }
            catch(Exception err)
            {
                _sLog.Error($"Exception:\n{err.ToString()}");
                SetMessage(err.ToString(), MessageType.Error);
            }
        }
        class ItemSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                SisEntry entry1 = x as SisEntry;
                SisEntry entry2 = y as SisEntry;
                if (entry1!=null && entry1.Name!= null)
                    return entry1.Name.CompareTo(entry2?.Name);

                return 0;
            }
        }

        private bool CanList() => true;
        private void OnAcceptAutentication(object ctl)
        {
            PasswordBox pb = ctl as PasswordBox;
            if (pb != null)
            {
                _decoder = pb.SecurePassword;
                string pass = _decoder.convertToUNSecureString();
                PassLst = Kpr.Load(FilePath, pass);
                if (PassLst != null)
                {
                    IsAutenticate = true;
                    _timer.Enabled = true;
                    SetMessage("File loaded OK", MessageType.Message);
                    pb.Clear();
                    OnList();
                }
                else
                {
                    IsAutenticate = false;
                    SetMessage("File fail to load", MessageType.Error);
                }
                _sLog.Debug($"OnAcceptAutentication IsAutenticate={IsAutenticate}");
            }
        }
        private bool CanAcceptAutentication(object ctl)
        {
            return PassLst==null && !string.IsNullOrEmpty(FilePath);
        }
        private void OnBrowse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                cmdAcceptAutentication.RaiseCanExecuteChanged();
            }

        }
        private bool CanBrowse()
        {
            return true;
        }
        private bool CanEdit()
        {
            return true;
        }


        private void OnEdit()
        {
            if (NotInEditMode)
                SetEditMode(true);
            else
                SetEditMode(false);
        }

        #endregion Commands


        #region Properties
        Timer _timer;
        bool _IsAutenticate;
        public bool IsAutenticate
        {
            get { return _IsAutenticate; }
            set
            {
                _IsAutenticate = value;
                RaisePropertyChanged();
            }
        }

        PassLst _passList;
        internal PassLst PassLst
        {
            get => _passList;
            private set
            {
                _passList = value;
                RaisePropertyChanged();
            }
        }
        string _filePath;
        public string FilePath
        {
            get { return _filePath; } 
            set
            {
                _filePath = value;
                RaisePropertyChanged();
            }
        }
        SecureString _decoder;
        public ICollectionView ItemList
        {
            get; 
            private set;
        }

        string _FilterPattern;
        public string FilterPattern
        {
            get => _FilterPattern;
            set
            {
                _FilterPattern = value;
                RaisePropertyChanged();
                OnList();
            }
        }
        SisEntry _editEntry;
        SisEntry _SelectedEntry;
        public SisEntry SelectedEntry
        {
            get => _SelectedEntry;
            set
            {
                _SelectedEntry = value;
                RaisePropertyChanged();
                cmdAdd?.RaiseCanExecuteChanged();
            }
        }

        OutputMassageVM _outpuMessage;
        public OutputMassageVM OutputMessage
        {
            get => _outpuMessage;
            set
            {
                _outpuMessage = value;
                RaisePropertyChanged();
            }
        }

        bool _NotInEditMode;
        public bool NotInEditMode
        {
            get => _NotInEditMode;
            set
            {
                _NotInEditMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged("EditText");
            }
        }
        public string EditText
        {
            get
            {
                if (NotInEditMode)
                    return "Edit";
                else
                    return "Exit Edit";
            }
        }

        bool _isNameReadOnly;
        public bool IsNameReadOnly
        {
            get => _isNameReadOnly;
            set
            {
                _isNameReadOnly = value;
                RaisePropertyChanged();
            }
        }
        bool _isDataReadOnly;
        public bool IsDataReadOnly
        {
            get => _isDataReadOnly;
            set
            {
                _isDataReadOnly = value;
                RaisePropertyChanged();
            }
        }
        #endregion Properties

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (PassLst != null)
                {
                    PassLst = null;
                    IsAutenticate = false;
                    _timer.Enabled = false;
                    _decoder = null;
                    _sLog.Debug($"timeout");
                }
            });

        }
        public void SetMessage(string msg, MessageType type)
        {
            OutputMessage = new OutputMassageVM() { Message = msg, Type = type };
            if (type == MessageType.Error)
                _sLog.Error(msg);
            else
                _sLog.Debug(msg);
        }
        public void ReSetMessage(string msg, MessageType type)
        {
            OutputMessage = new OutputMassageVM() ;
        }

        private bool CanActivatEditMode(bool activateEditMode)
        {
            if (activateEditMode)
                return _editEntry == null && _SelectedEntry != null;
            else
                return _editEntry != null && _SelectedEntry != _editEntry;
        }
        internal void SetEditMode(bool activateEditMode)
        {
            if (activateEditMode)
            {
                _editEntry = SelectedEntry;
                SisEntry temp = new SisEntry(SelectedEntry);
                SelectedEntry = temp;
                SetMessage("In edit mode", MessageType.Message);
                NotInEditMode = false;
                ActivatEditUI(false, true);
            }
            else
            {
                _editEntry = null;
                SetMessage("Exit edit mode", MessageType.Message);
                NotInEditMode = true;
                ActivatEditUI(false, false);
            }
            cmdEdit.RaiseCanExecuteChanged();
            cmdUpdate.RaiseCanExecuteChanged();

        }

        private void ActivatEditUI(bool name, bool otherData)
        {
            IsNameReadOnly = !name;
            IsDataReadOnly = !otherData;
        }


        private void Load(string output, Cmds cmd)
        {
            //string[] arr = output.Split(new Char[] { ',', '\r', ' ', '\n' });
            //Match m = null;
            //switch (cmd)
            //{
            //    case Cmds.Find:
            //        foreach (string s in arr)
            //        {
            //            if (!string.IsNullOrEmpty(s))
            //            {
            //                m = Regex.Match(s, @"(?<key>[A-Za-z]+)=\[(?<val>.*)\]");
            //                if (m.Success)
            //                {
            //                    string key = m.Groups["key"].Value;
            //                    if (key == "Name")
            //                        Name = m.Groups["val"].Value;
            //                    else if (key == "Pass")
            //                        Pass = m.Groups["val"].Value;
            //                    else if (key == "User")
            //                        User = m.Groups["val"].Value;
            //                    else if (key == "Note")
            //                        Note = m.Groups["val"].Value;
            //                }
            //            }
            //        }
            //        break;
            //}
        }

        //private void Run(Cmds cmd)
        //{
        //    Process proc = new Process();
        //    proc.StartInfo.FileName = "kpr.exe";
        //    proc.StartInfo.Arguments = BuildCmdLine(cmd);
        //    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //    proc.StartInfo.UseShellExecute = false;
        //    proc.StartInfo.RedirectStandardOutput = true;
        //    proc.Start();

        //    // Synchronously read the standard output of the spawned process. 
        //    StreamReader reader = proc.StandardOutput;
        //    string output = reader.ReadToEnd();

        //    proc.WaitForExit();

        //    Load(output, cmd);

        //}

        private  string BuildCmdLine(Cmds cmd)
        {
            StringBuilder sb = new StringBuilder();
            //switch (cmd)
            //{
            //    case Cmds.Find:
            //        sb.Append("3r.kpr");
            //        sb.Append(" ");
            //        sb.Append(Pass);
            //        sb.Append(" ");
            //        sb.Append(cmd);
            //        sb.Append(" ");
            //        sb.Append(Name);
            //        break;
            //}
            return sb.ToString();
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }
        #endregion
    }

    internal enum Cmds { None, Find,Add,List};
}
