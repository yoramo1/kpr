using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;

namespace GeneralServices
{
    public class CryptoToken
    {
        public virtual byte[] GetInitVector()
        {
            HashAlgorithm.Create("");
            return new byte[100];
        }

        public virtual byte[] GetKey()
        {
            return new byte[100];
        }
    }

    public class IsecCryptoToken : CryptoToken
    {
        //private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private byte[] key = (byte[])null;
        private byte[] iv = (byte[])null;
        private Encoding _encoding = Encoding.Unicode;

        static IsecCryptoToken()
        {
        }

        public IsecCryptoToken(SecureString sToken)
        {
            //XmlConfigurator.Configure();
            this.CreateKeyAndIV(sToken.ToString(), Encoding.ASCII.GetBytes("HR$2pIjHR$2pIj12"), out this.key, out this.iv);
        }

        private void CreateKeyAndIV(string sSecret, byte[] salt, out byte[] retKey, out byte[] retIV)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("secret");
            MD5 md5 = MD5.Create();
            int length = bytes.Length + salt.Length;
            byte[] buffer1 = new byte[length];
            Buffer.BlockCopy((Array)bytes, 0, (Array)buffer1, 0, bytes.Length);
            Buffer.BlockCopy((Array)salt, 0, (Array)buffer1, bytes.Length, salt.Length);
            byte[] hash1 = md5.ComputeHash(buffer1);
            byte[] buffer2 = new byte[hash1.Length + length];
            Buffer.BlockCopy((Array)hash1, 0, (Array)buffer2, 0, hash1.Length);
            Buffer.BlockCopy((Array)buffer1, 0, (Array)buffer2, hash1.Length, buffer1.Length);
            byte[] hash2 = md5.ComputeHash(buffer2);
            md5.Clear();
            retKey = hash1;
            retIV = hash2;
        }

        private byte[] HashBytes(string sPassword, Encoding enc, string sHashAlgo)
        {
            using (HashAlgorithm hashAlgorithm = HashAlgorithm.Create(sHashAlgo))
                return hashAlgorithm.ComputeHash(enc.GetBytes(sPassword));
        }

        private static byte[] GetBytes_MD5(string text)
        {
            return new MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(text));
        }

        public string Reverse(string s)
        {
            Stack<string> stack = new Stack<string>();
            for (int startIndex = 0; startIndex < s.Length; ++startIndex)
                stack.Push(s.Substring(startIndex, 1));
            string str = string.Empty;
            for (int index = 0; index < s.Length; ++index)
                str = str + stack.Pop();
            return str;
        }

        public override byte[] GetInitVector()
        {
            return this.iv;
        }

        public override byte[] GetKey()
        {
            return this.key;
        }
    }

    public class ObjectSerialization<T>
    {
        //private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static ObjectSerialization()
        {
        }

        public ObjectSerialization()
        {
            //XmlConfigurator.Configure();
        }

        public static T Load(string path, CryptoToken token)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            return ObjectSerialization<T>.Load(path, token, xs);
        }

        public static T Load(string path, CryptoToken token, XmlSerializer xs)
        {
            Debug.WriteLine("ObjectSerialization<" + typeof(T).ToString() + ">.Load->" + path);
            T obj = default(T);
            FileStream fileStream = (FileStream)null;
            try
            {
                if (token == null)
                {
                    using (StreamReader streamReader = new StreamReader(path))
                        obj = (T)xs.Deserialize((TextReader)streamReader);
                }
                else
                {
                    fileStream = File.Open(path, FileMode.Open);
                    Rijndael rijndael = Rijndael.Create();
                    using (StreamReader streamReader = new StreamReader((Stream)new CryptoStream((Stream)fileStream, rijndael.CreateDecryptor(token.GetKey(), token.GetInitVector()), CryptoStreamMode.Read)))
                    {
                        StringReader stringReader = new StringReader(streamReader.ReadToEnd());
                        obj = (T)xs.Deserialize((TextReader)stringReader);
                    }
                }
            }
            catch (CryptographicException ex)
            {
                //ObjectSerialization<T>.logger.Error((object)("Exception in " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message));
                Console.WriteLine("Error decripting file [{0}]", (object)path);
            }
            catch (Exception ex)
            {
                //ObjectSerialization<T>.logger.Error((object)("Exception in " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message));
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
            return obj;
        }

        public static bool Save(T obj, string path, CryptoToken token)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            return ObjectSerialization<T>.Save(obj, xs, path, token);
        }

        public static bool Save(T obj, XmlSerializer xs, string path, CryptoToken token)
        {
            bool flag = false;
            FileStream fileStream = (FileStream)null;
            Debug.WriteLine("ObjectSerialization<" + typeof(T).ToString() + ">.Save->" + path);
            try
            {
                if (token != null)
                {
                    fileStream = File.Open(path, FileMode.Create);
                    ICryptoTransform encryptor = Rijndael.Create().CreateEncryptor(token.GetKey(), token.GetInitVector());
                    using (StreamWriter streamWriter = new StreamWriter((Stream)new CryptoStream((Stream)fileStream, encryptor, CryptoStreamMode.Write)))
                        xs.Serialize((TextWriter)streamWriter, (object)obj);
                }
                else
                {
                    using (StreamWriter streamWriter = new StreamWriter(path))
                        xs.Serialize((TextWriter)streamWriter, (object)obj);
                }
                flag = true;
            }
            catch (CryptographicException ex)
            {
                //ObjectSerialization<T>.logger.Error((object)("CryptographicException in " + MethodBase.GetCurrentMethod().Name + " : " + ex.ToString()));
                flag = false;
            }
            catch (Exception ex)
            {
                //ObjectSerialization<T>.logger.Error((object)("Exception in " + MethodBase.GetCurrentMethod().Name + " : " + ex.ToString()));
                flag = false;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
            return flag;
        }
    }

}

