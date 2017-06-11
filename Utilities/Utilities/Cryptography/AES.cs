using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
    public static class AES
    {
        public static byte[] Encrypt(byte[] value, byte[] key, byte[] iv, int keyLength, int blockLength, int feedbackSize, CipherMode mode, PaddingMode paddingMode)
        {
            using (var cipher = CreateCipher(key, iv, keyLength, blockLength, feedbackSize, mode, paddingMode))
            {
                return Encrypt(cipher, value);
            }
        }
        public static byte[] Decrypt(byte[] value, byte[] key, byte[] iv, int keyLength, int blockLength, int feedbackSize, CipherMode mode, PaddingMode paddingMode)
        {
            using (var cipher = CreateCipher(key, iv, keyLength, blockLength, feedbackSize, mode, paddingMode))
            {
                return Decrypt(cipher, value);
            }
        }

        public static byte[] Encrypt(RijndaelManaged cipher, byte[] value)
        {
            byte[] encryptedBytes;
            using (var encryptor = cipher.CreateEncryptor())
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var bw = new BinaryWriter(cs, Encoding.UTF8))
            {
                bw.Write(value);
                bw.Close();

                encryptedBytes = ms.ToArray();
            }
            return encryptedBytes;
        }

        public static byte[] Decrypt(RijndaelManaged cipher, byte[] value)
        {
            byte[] decryptedBytes;
            using (var decryptor = cipher.CreateDecryptor())
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            using (var bw = new BinaryWriter(cs, Encoding.UTF8))
            {
                bw.Write(value);
                bw.Close();

                decryptedBytes = ms.ToArray();
            }
            return decryptedBytes;
        }

        public static RijndaelManaged CreateCipher(byte[] key, byte[] iv, int keyLength, int blockSize, int feedbackSize, CipherMode mode, PaddingMode paddingMode)
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.Mode = mode;
            cipher.Padding = paddingMode;
            cipher.Key = key;
            cipher.IV = iv;
            cipher.KeySize = keyLength;
            cipher.BlockSize = blockSize;
            cipher.FeedbackSize = feedbackSize;

            return cipher;
        }
    }
}
