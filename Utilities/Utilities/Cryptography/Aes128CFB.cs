using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
    public static class Aes128CFB
    {
        public static byte[] Encrypt(byte[] text, byte[] key)
        {
            int blockSize = 128;
            int keySize = 128;

            // Encode the text
            string textEnc = Convert.ToBase64String(text);
            byte[] textBytes = Encoding.UTF8.GetBytes(textEnc);

            // cipherArray
            int cipherLen = (blockSize / 8) + textEnc.Length;
            byte[] cipherText = new byte[cipherLen];
            Array.Clear(cipherText, 0, cipherLen);

            // Create the IV needed for this operation
            string ivStr = StringHelper.RandomString(blockSize / 8);
            byte[] ivBytes = Encoding.UTF8.GetBytes(ivStr);

            // copy IV to the cipher text start
            ivBytes.CopyTo(cipherText, 0);

            // Process the cipher
            ProcessCipher(true, textBytes, key, ivBytes, blockSize, keySize, ref cipherText, blockSize / 8);

            return cipherText;
        }

        public static byte[] Decrypt(byte[] text, byte[] key)
        {
            int blockSize = 128;
            int keySize = 128;

            // Grab the IV and encrypted text from the original text
            byte[] ivBytes = text.Take(blockSize / 8).ToArray();
            text = text.Skip(blockSize / 8).Take(text.Length - (blockSize / 8)).ToArray();

            // Process the cipher
            ProcessCipher(false, text, key, ivBytes, blockSize, keySize, ref text, 0);

            string encodedText = Encoding.UTF8.GetString(text);
            return Convert.FromBase64String(encodedText);
        }

        public static void ProcessCipher(bool encrypt, byte[] text, byte[] key, byte[] iv, int blockSize, int keySize, ref byte[] output, int offset)
        {
            using (var cipher = new RijndaelManaged())
            {
                cipher.BlockSize = blockSize;
                cipher.KeySize = keySize;

                cipher.Mode = CipherMode.CFB;
                cipher.FeedbackSize = 8;
                cipher.Padding = PaddingMode.None;

                cipher.Key = key;
                cipher.IV = iv;

                using (var encryptor = (encrypt) ? cipher.CreateEncryptor() : cipher.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var bw = new BinaryWriter(cs, Encoding.UTF8))
                {
                    bw.Write(text);
                    bw.Close();

                    ms.ToArray().CopyTo(output, offset);
                }
            }
        }
    }
}
