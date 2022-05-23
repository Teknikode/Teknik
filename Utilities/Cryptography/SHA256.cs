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
            var hash = System.Security.Cryptography.SHA256.Create();
            byte[] hashBytes = hash.ComputeHash(value);

            return Convert.ToBase64String(hashBytes);
        }

        public static byte[] Hash(Stream value)
        {
            var hash = System.Security.Cryptography.SHA256.Create();
            return hash.ComputeHash(value);
        }

        public static string Hash(string value, string salt1, string salt2)
        {
            var hash = System.Security.Cryptography.SHA256.Create();
            // gen salt2 hash
            byte[] dataSalt2 = Encoding.UTF8.GetBytes(salt2);
            byte[] salt2Bytes = hash.ComputeHash(dataSalt2);
            string salt2Str = string.Empty;
            foreach (byte x in salt2Bytes)
            {
                salt2Str += string.Format("{0:x2}", x);
            }
            string dataStr = salt1 + value + salt2Str;
            string sha1Str = SHA1.Hash(dataStr);
            byte[] sha1Bytes = Encoding.UTF8.GetBytes(sha1Str);
            byte[] valueBytes = hash.ComputeHash(sha1Bytes);
            string hashString = string.Empty;
            foreach (byte x in valueBytes)
            {
                hashString += string.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}
