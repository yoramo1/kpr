// Type: kpr.KprCryptoToken
// Assembly: kpr, Version=1.0.0.1, Culture=neutral, PublicKeyToken=null
// Assembly location: F:\y\m\kpr.exe

using GeneralServices;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace kpr
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            KprPerformer performer = new KprPerformer();
            if (args.Length <= 2)
            {
                Program.PrintUsage();
            }
            else
            {
                Program.ProcessParams(args, ref performer);
                performer.Action();
            }
        }

        private static bool ProcessParams(string[] args, ref KprPerformer performer)
        {
            performer.Path = args[0];
            performer.Pass = args[1];
            performer.Cmd = args[2];
            for (int index = 3; index < args.Length; ++index)
                performer.Params.Add(args[index]);
            return true;
        }

        private static void PrintUsage()
        {
            string[] strArray = new string[7]
                  {
                    "Kpr <path> <pass> [options]",
                    "   ADD name pass user note url  - Add a new Entry",
                    "   C                            - create",
                    "   FIND <name>                  - find name",
                    "   LIST                         - list",
                    "   SET name pass user note url  - set values to a entry",
                    "*  DEL name                     - delete a entry"
                  };
            foreach (string str in strArray)
                Console.WriteLine(str);
        }
    }

    public class KprCryptoToken : CryptoToken
    {
        public string Path { get; set; }

        public string Pass { get; set; }

        public KprCryptoToken(string path, string pass)
        {
            this.Path = path;
            this.Pass = pass;
        }

        public override byte[] GetInitVector()
        {
            return new byte[18]      {
                                        (byte) 13,
                                        (byte) 22,
                                        (byte) 53,
                                        (byte) 98,
                                        (byte) 44,
                                        (byte) 70,
                                        (byte) 46,
                                        (byte) 35,
                                        (byte) 16,
                                        (byte) 6,
                                        (byte) 36,
                                        (byte) 57,
                                        (byte) 15,
                                        (byte) 76,
                                        (byte) 93,
                                        (byte) 17,
                                        (byte) 1,
                                        (byte) 3
                                      };
        }

        public override byte[] GetKey()
        {
            char[] chArray = this.Pass.ToCharArray();
            byte[] buffer = new byte[16];
            MD5 md5 = MD5.Create();
            for (int index = 0; index < chArray.Length; ++index)
                buffer[index] = (byte)chArray[index];
            return md5.ComputeHash(buffer);
        }

        private byte[] HashBytes(string sPassword, Encoding enc, string sHashAlgo)
        {
            using (HashAlgorithm hashAlgorithm = HashAlgorithm.Create(sHashAlgo))
                return hashAlgorithm.ComputeHash(enc.GetBytes(sPassword));
        }
    }

    public class SisEntry
    {
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

        [XmlAttribute]
        public string[] OldPass { get; set; }

        [XmlAttribute]
        public DateTime LastUpdated { get; set; }
    }

    internal class KprPerformer
    {
        private PassLst _lst;

        internal string Path { get; set; }

        internal string Pass { get; set; }

        internal string Cmd { get; set; }

        internal List<string> Params { get; set; }

        internal KprPerformer()
        {
            this._lst = (PassLst)null;
            this.Params = new List<string>();
        }

        internal void Action()
        {
            if (this.Pass == "?")
            {
                Console.Write("Enter pass:  ");
                this.Pass = Console.ReadLine();
            }
            switch (this.Cmd.ToUpper())
            {
                case "ADD":
                    this._lst = this.Load();
                    if (this._lst == null)
                        break;
                    this.AddEntryAction();
                    break;
                case "C":
                    this.CreateAction();
                    break;
                case "DEL":
                    this._lst = this.Load();
                    if (this._lst == null)
                        break;
                    this.DeleteEntryAction();
                    break;
                case "FIND":
                    this._lst = this.Load();
                    if (this._lst == null)
                        break;
                    this.FindAction();
                    break;
                case "LIST":
                    this._lst = this.Load();
                    if (this._lst == null)
                        break;
                    this.ListAction();
                    break;
                case "DUMP":
                    this._lst = this.Load();
                    if (this._lst == null)
                        break;
                    this.DumpAction();
                    break;
                case "SET":
                    this._lst = this.Load();
                    if (this._lst == null)
                        break;
                    this.SetEntryAction();
                    break;
                default:
                    Console.WriteLine("Unknown command!! [{0}]", (object)this.Cmd);
                    break;
            }
        }

        private void DumpAction()
        {
            SortedList<string, SisEntry> objSortedList = new SortedList<string, SisEntry>();
            foreach (SisEntry sisEntry in this._lst.Items)
                objSortedList.Add(sisEntry.Name, sisEntry);

            foreach (string str in objSortedList.Keys)
            {
                SisEntry entry = objSortedList[str];
                Console.WriteLine(string.Format("   {0} - {1}", str, entry.Pass));
            }
        }

        private PassLst Load()
        {
            if (!File.Exists(this.Path) && new FileInfo(this.Path).Extension != "kpr")
            {
                KprPerformer kprPerformer = this;
                string str = kprPerformer.Path + ".kpr";
                kprPerformer.Path = str;
            }
            Rijndael.Create().GenerateIV();
            return ObjectSerialization<PassLst>.Load(this.Path, (CryptoToken)new KprCryptoToken(this.Path, this.Pass));
        }

        private void Save()
        {
            Rijndael.Create().GenerateIV();
            ObjectSerialization<PassLst>.Save(this._lst, this.Path, (CryptoToken)new KprCryptoToken(this.Path, this.Pass));
        }

        private SisEntry FindEntry(string str)
        {
            SisEntry sisEntry1 = (SisEntry)null;
            foreach (SisEntry sisEntry2 in this._lst.Items)
            {
                if (sisEntry2.Name.ToUpper() == this.Params[0].ToUpper())
                {
                    sisEntry1 = sisEntry2;
                    break;
                }
            }
            return sisEntry1;
        }

        private List<SisEntry> FindAllEntry(string str)
        {
            List<SisEntry> list = new List<SisEntry>();
            foreach (SisEntry sisEntry in this._lst.Items)
            {
                if (sisEntry.Name == this.Params[0])
                    list.Add(sisEntry);
            }
            return list;
        }

        private void CreateAction()
        {
            if (File.Exists(this.Path))
            {
                Console.WriteLine("File exists can not overide!");
            }
            else
            {
                Rijndael.Create().GenerateIV();
                ObjectSerialization<PassLst>.Save(new PassLst(), this.Path, (CryptoToken)new KprCryptoToken(this.Path, this.Pass));
            }
        }

        private void AddEntryAction()
        {
            SisEntry entry = this.FindEntry(this.Params[0]);
            if (entry != null)
            {
                Console.WriteLine("Entry already exists! [{0}]", (object)entry.Name);
            }
            else
            {
                SisEntry sisEntry = new SisEntry();
                sisEntry.Name = this.Params[0];
                sisEntry.Pass = this.Params[1];
                if (this.Params.Count > 2)
                    sisEntry.User = this.Params[2];
                if (this.Params.Count > 3)
                    sisEntry.Note = this.Params[3];
                if (this.Params.Count > 4)
                    sisEntry.Url = this.Params[4];
                sisEntry.LastUpdated = DateTime.Now;
                this._lst.Items.Add(sisEntry);
                this.Save();
            }
        }

        private void ListAction()
        {
            SortedList<string, SisEntry> objSortedList = new SortedList<string, SisEntry>();
            foreach (SisEntry sisEntry in this._lst.Items)
                objSortedList.Add(sisEntry.Name, sisEntry);

            foreach (string str in objSortedList.Keys)
            {
                SisEntry entry = objSortedList[str];
                Console.WriteLine(string.Format("   {0,-50}\t{1}", str, GetLastUpdated(entry)));
            }
        }

        private string GetLastUpdated(SisEntry entry)
        {
            if (entry!= null)
            {
                if (entry.LastUpdated == DateTime.MinValue)
                {
                    return "----";
                }
                else
                {
                    return entry.LastUpdated.ToShortDateString();
                }
            }
            return "---";
        }

        private void FindAction()
        {
            SisEntry entry = this.FindEntry("Not Found!");
            if (entry != null)
            {
                Console.WriteLine(string.Format("Name=[{0}], Pass=[{2}],User=[{1}]", (object)entry.Name, (object)entry.User, (object)entry.Pass));
                Console.WriteLine(string.Format("Note=[{0}], Url=[{1}]", (object)entry.Note, (object)entry.Url));
                if (entry.OldPass != null)
                {
                    Console.Write("Old Pass=[ ");
                    foreach (string str in entry.OldPass)
                    {
                        Console.Write(str);
                        Console.Write(", ");
                    }
                    Console.Write(" ]");
                }
                Clipboard.SetText(entry.Pass);
            }
            else
                Console.WriteLine("Name not found: [{0}]", (object)this.Params[0]);
        }

        private void SetEntryAction()
        {
            SisEntry entry = this.FindEntry(this.Params[0]);
            if (entry != null)
            {
                List<string> lst = new List<string>();
                if (entry.OldPass == null || entry.OldPass.Length == 0)
                {
                }
                else
                {
                    lst.AddRange(entry.OldPass);
                }

                lst.Add(entry.Pass);
                entry.OldPass = lst.ToArray();

                entry.Name = this.Params[0];
                entry.Pass = this.Params[1];
                if (this.Params.Count > 2)
                    entry.User = this.Params[2];
                if (this.Params.Count > 3)
                    entry.Note = this.Params[3];
                if (this.Params.Count > 4)
                    entry.Url = this.Params[4];
                entry.LastUpdated = DateTime.Now;
                this.Save();
            }
            else
                Console.WriteLine("Entry does not exist! [{0}]", (object)entry.Name);
        }

        private void DeleteEntryAction()
        {
            SisEntry entry = this.FindEntry(this.Params[0]);
            if (entry != null)
            {
                if (!this._lst.Items.Remove(entry))
                    return;
                this.Save();
                Console.WriteLine("Entry Removed: [{0}]", (object)this.Params[0]);
            }
            else
                Console.WriteLine("Entry does not exist! [{0}]", (object)this.Params[0]);
        }
    }


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
}
