using System.Text;
using SecurityDriven.Inferno.Hash;
using SecurityDriven.Inferno.Mac;
using System.IO;
using System.Security.Cryptography;
using Teknik.Configuration;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace Teknik.Helpers
{
    public class MD5
    {
        public static string Hash(string value)
        {
            byte[] valBytes = Encoding.ASCII.GetBytes(value);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hashBytes = md5.ComputeHash(valBytes);

            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sBuilder.Append(hashBytes[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }
    }

    public class SHA384
    {
        public static string Hash(string key, string value)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] data = Encoding.ASCII.GetBytes(value);

            byte[] result = new HMAC2(HashFactories.SHA384, keyBytes).ComputeHash(data);

            return Encoding.ASCII.GetString(result);
        }
    }

    public class SHA256
    {
        public static string Hash(string value, string salt1, string salt2)
        {
            SHA256Managed hash = new SHA256Managed();
            SHA1 sha1 = new SHA1Managed();
            // gen salt2 hash
            byte[] dataSalt2 = Encoding.UTF8.GetBytes(salt2);
            byte[] salt2Bytes = hash.ComputeHash(dataSalt2);
            string salt2Str = string.Empty;
            foreach (byte x in salt2Bytes)
            {
                salt2Str += String.Format("{0:x2}", x);
            }
            string dataStr = salt1 + value + salt2Str;
            byte[] dataStrBytes = Encoding.UTF8.GetBytes(dataStr);
            byte[] shaBytes = sha1.ComputeHash(dataStrBytes);
            string sha1Str = string.Empty;
            foreach (byte x in shaBytes)
            {
                sha1Str += String.Format("{0:x2}", x);
            }
            byte[] sha1Bytes = Encoding.UTF8.GetBytes(sha1Str);
            byte[] valueBytes = hash.ComputeHash(sha1Bytes);
            string hashString = string.Empty;
            foreach (byte x in valueBytes)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }

    public class AES
    {
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Decrypt(data, key, iv, "CTR", "NoPadding");
        }
        public static byte[] Decrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes, "CTR", "NoPadding");
        }
        public static byte[] DecryptCBC(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes, "CBC", "PKCS5PADDING");
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, string mode, string padding)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/" + mode + "/" + padding);

            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher.DoFinal(data);
        }


        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Encrypt(data, key, iv, "CTR", "NoPadding");
        }
        public static byte[] Encrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Encrypt(data, keyBytes, ivBytes, "CTR", "NoPadding");
        }
        public static byte[] EncryptCBC(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Encrypt(data, keyBytes, ivBytes, "CBC", "PKCS5PADDING");
        }
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, string mode, string padding)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/" + mode + "/" + padding);

            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher.DoFinal(data);
        }

        public static byte[] CreateKey(string password, string iv, int keySize = 256)
        {
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return CreateKey(password, ivBytes, keySize);
        }
        public static byte[] CreateKey(string password, byte[] iv, int keySize = 256)
        {
            const int Iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, iv, Iterations);
            return keyGenerator.GetBytes(keySize / 8);
        }
    }
}