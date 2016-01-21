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

    public class AES
    {
        public static byte[] Decrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes);
        }
        public static byte[] Decrypt(byte[] data, string key, string iv, int keySize, int blockSize)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes, keySize, blockSize);
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Decrypt(data, key, iv, 256, 128);
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, int keySize, int blockSize)
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
        public static byte[] Encrypt(byte[] data, string key, string iv, int keySize, int blockSize)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Encrypt(data, keyBytes, ivBytes, keySize, blockSize);
        }
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Encrypt(data, key, iv, 256, 128);
        }
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, int keySize, int blockSize)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher.DoFinal(data);
        }
    }
}