using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace wKpr.Model
{
    public class Kpr
    {
        private static readonly NLog.Logger _sLog = NLog.LogManager.GetCurrentClassLogger();
        static internal PassLst Load(string path, string pass)
        {
            if (!File.Exists(path) && new FileInfo(path).Extension != "kpr")
            {
                //KprPerformer kprPerformer = this;
                //string str = kprPerformer.Path + ".kpr";
                //kprPerformer.Path = str;
            }
            Rijndael.Create().GenerateIV();
            PassLst ret = ObjectSerialization<PassLst>.Load(path, (CryptoToken)new KprCryptoToken(path, pass));
            if (ret!=null)
            {
                foreach (SisEntry se in ret.Items)
                    se.State = EntryState.Loaded;
            }
            return ret;
        }

        static internal void Save(PassLst lst,string path,string pass)
        {
            _sLog.Debug($"Save");
            Rijndael.Create().GenerateIV();
            ObjectSerialization<PassLst>.Save(lst, path, (CryptoToken)new KprCryptoToken(path, pass));
        }

        #region services
        internal static SisEntry Find(string name, PassLst list)
        {
            SisEntry ret = null;
            if (list != null)
            {
                foreach (SisEntry se in list.Items)
                {
                    if (se.Name == name)
                    {
                        ret = se;
                        break;
                    }
                }
            }
            return ret;
        }
        internal static bool Add(SisEntry se, PassLst list)
        {
            bool ret = false;
            if (list != null)
            {
                SisEntry found = Find(se.Name, list);
                if (found== null)
                {
                    list.Items.Add(se);
                    ret = true;
                }
            }
            return ret;
        }

        internal static bool Update(SisEntry se, PassLst list)
        {
            if (se!=null && se.State== EntryState.Loaded)
            {

            }
            return false;
        }
        #endregion


        public class PassLst
        {
            public List<SisEntry> Items { get; set; }

            public string Hint { get; set; }

            public PassLst()
            {
                this.Items = new List<SisEntry>();
                this.Hint = "No Hint";
            }
        }

        internal static void UpdateEntry(SisEntry originalEntry, SisEntry updatedEntry, PassLst passList)
        {
            _sLog.Debug($"UpdateEntry");
            if (originalEntry.Pass != updatedEntry.Pass || true)
            {
                SisEntry foundEntry = Find(originalEntry.Name, passList);
                if (foundEntry != null)
                {
                    List<string> lst = new List<string>();
                    if (foundEntry.OldPass == null || foundEntry.OldPass.Length == 0)
                    {
                    }
                    else
                    {
                        lst.AddRange(foundEntry.OldPass);
                    }
                    lst.Add(updatedEntry.Pass);
                    foundEntry.OldPass = lst.ToArray();
                    foundEntry.Pass = updatedEntry.Pass;
                }
                if (foundEntry.Note != updatedEntry.Note)
                    foundEntry.Note = updatedEntry.Note;
                if (foundEntry.User != updatedEntry.User)
                    foundEntry.User = updatedEntry.User;
                if (foundEntry.Url != updatedEntry.Url)
                    foundEntry.Url = updatedEntry.Url;

                foundEntry.LastUpdated = DateTime.Now;
            }
        }

        internal static void Remove(PassLst passList, SisEntry selectedEntry)
        {
            if (passList!= null && selectedEntry!= null)
            {
                var found = Find(selectedEntry.Name, passList);
                if (found != null)
                    passList.Items.Remove(found);
            }
        }
    }
}
