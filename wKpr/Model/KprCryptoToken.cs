using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace wKpr.Model
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

}
