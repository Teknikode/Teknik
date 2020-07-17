using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Teknik.Utilities.Cryptography
{
    public class SHA256
    {
        public static string Hash(string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return Hash(valueBytes);
        }

        public static string Hash(byte[] value)
        {
            HashAlgorithm hash = new SHA256CryptoServiceProvider();
            byte[] hashBytes = hash.ComputeHash(value);

            return Convert.ToBase64String(hashBytes);
        }

        public static byte[] Hash(Stream value)
        {
            HashAlgorithm hash = new SHA256CryptoServiceProvider();
            return hash.ComputeHash(value);
        }

        public static string Hash(string value, string salt1, string salt2)
        {
            SHA256Managed hash = new SHA256Managed();
            // gen salt2 hash
            byte[] dataSalt2 = Encoding.UTF8.GetBytes(salt2);
            byte[] salt2Bytes = hash.ComputeHash(dataSalt2);
            string salt2Str = string.Empty;
            foreach (byte x in salt2Bytes)
            {
                salt2Str += String.Format("{0:x2}", x);
            }
            string dataStr = salt1 + value + salt2Str;
            string sha1Str = SHA1.Hash(dataStr);
            byte[] sha1Bytes = Encoding.UTF8.GetBytes(sha1Str);
            byte[] valueBytes = hash.ComputeHash(sha1Bytes);
            string hashString = string.Empty;
            foreach (byte x in valueBytes)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static System.Security.Cryptography.SHA256 CreateHashAlgorithm()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                return new SHA256CryptoServiceProvider();
            }

            return new SHA256Managed();
        }
    }
}
