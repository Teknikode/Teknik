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


namespace Teknik.Helpers
{
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
            string dataStr = salt1 + value + salt2;
            byte[] dataStrBytes = Encoding.ASCII.GetBytes(dataStr);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] valueBytes = sha.ComputeHash(dataStrBytes);
            byte[] result = new HMAC2(HashFactories.SHA256).ComputeHash(valueBytes);

            return Encoding.ASCII.GetString(result);
        }
    }

    public class AES
    {
        public static byte[] Decrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes);
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher.DoFinal(data);
        }

        public static byte[] Encrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Encrypt(data, keyBytes, ivBytes);
        }
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

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