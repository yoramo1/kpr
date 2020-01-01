using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace wKpr.Model
{
    public class ObjectSerialization<T>
    {
        private static readonly NLog.Logger _sLog = NLog.LogManager.GetCurrentClassLogger();
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
                _sLog.Error($"Error decrypting file CryptographicException [{path}] - {ex.ToString()}" );
            }
            catch (Exception ex)
            {
                _sLog.Error($"Error decrypting file [{path}] - {ex.ToString()}");
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
            _sLog.Debug($"ObjectSerialization<{ typeof(T).ToString()} >.Save->{ path}");
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
                _sLog.Error($"Error save file [{(object)path}] - {ex.ToString()}");
                flag = false;
            }
            catch (Exception ex)
            {
                _sLog.Error($"Error save file [{(object)path}] - {ex.ToString()}");
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
