using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Teknik.Utilities.Cryptography
{

    public class AesCounterManaged
    {
        public static byte[] Decrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes);
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return ProcessCipher(false, stream, 1024, key, iv);
            }
        }
        
        public static byte[] Encrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Encrypt(data, keyBytes, ivBytes);
        }
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return ProcessCipher(true, stream, 1024, key, iv);
            }
        }

        public static byte[] ProcessCipher(bool encrypt, Stream input, int chunkSize, byte[] key, byte[] iv)
        {
            // Make sure the input stream is at the beginning
            input.Seek(0, SeekOrigin.Begin);

            AesCounterStream cryptoStream = new AesCounterStream(input, encrypt, key, iv);

            // Initialize variables
            byte[] output = new byte[input.Length];

            // Process the stream and save the bytes to the output
            int curByte = 0;
            int processedBytes = 0;
            byte[] buffer = new byte[chunkSize];
            int bytesRemaining = (int)input.Length;
            int bytesToRead = chunkSize;
            do
            {
                if (chunkSize > bytesRemaining)
                {
                    bytesToRead = bytesRemaining;
                }

                processedBytes = cryptoStream.Read(buffer, 0, bytesToRead);
                if (processedBytes > 0)
                {
                    buffer.Take(processedBytes).ToArray().CopyTo(output, curByte);

                    // Clear the buffer
                    Array.Clear(buffer, 0, chunkSize);
                }
                curByte += processedBytes;
                bytesRemaining -= processedBytes;
            }
            while (processedBytes > 0 && bytesRemaining > 0);

            return output;
        }

        public static void EncryptToFile(Stream input, string filePath, int chunkSize, byte[] key, byte[] iv)
        {

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                EncryptToStream(input, fileStream, chunkSize, key, iv);
            }
        }

        public static void EncryptToStream(Stream input, Stream output, int chunkSize, byte[] key, byte[] iv)
        {
            // Make sure the input stream is at the beginning
            input.Seek(0, SeekOrigin.Begin);

            AesCounterStream cryptoStream = new AesCounterStream(input, true, key, iv);

            int curByte = 0;
            int processedBytes = 0;
            byte[] buffer = new byte[chunkSize];
            int bytesRemaining = (int)input.Length;
            int bytesToRead = chunkSize;
            do
            {
                if (chunkSize > bytesRemaining)
                {
                    bytesToRead = bytesRemaining;
                }

                processedBytes = cryptoStream.Read(buffer, 0, bytesToRead);
                if (processedBytes > 0)
                {
                    output.Write(buffer, 0, processedBytes);

                    // Clear the buffer
                    Array.Clear(buffer, 0, chunkSize);
                }
                curByte += processedBytes;
                bytesRemaining -= processedBytes;
            }
            while (processedBytes > 0 && bytesRemaining > 0);
        }

        public static byte[] CreateKey(string password, string iv, int keySize = 256)
        {
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return CreateKey(password, ivBytes, keySize);
        }
        public static byte[] CreateKey(string password, byte[] iv, int keySize = 256)
        {
            const int Iterations = 5000;
            var keyGenerator = new Rfc2898DeriveBytes(password, iv, Iterations);
            return keyGenerator.GetBytes(keySize / 8);
        }
    }
}
