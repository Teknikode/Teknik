using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
    public class AesCounterManaged
    {
        public static async Task EncryptToFile(Stream input, string filePath, byte[] key, byte[] iv)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await EncryptToStream(input, fileStream, key, iv);
            }
        }

        public static async Task EncryptToStream(Stream input, Stream output, byte[] key, byte[] iv)
        {
            // Make sure the input stream is at the beginning
            if (input.CanSeek)
                input.Seek(0, SeekOrigin.Begin);

            using (AesCounterStream cryptoStream = new AesCounterStream(input, true, key, iv))
            {
                await cryptoStream.CopyToAsync(output);
            }
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
