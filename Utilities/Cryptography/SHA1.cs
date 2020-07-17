using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Teknik.Utilities.Cryptography
{
    public class SHA1
    {
        public static string Hash(string text)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                return Hash(ms);
            }
        }

        public static string Hash(Stream stream)
        {
            var hash = default(string);
            using (var algo = System.Security.Cryptography.SHA1.Create())
            {
                var hashBytes = algo.ComputeHash(stream);

                // Return as hexadecimal string
                hash = hashBytes.ToHex();
            }
            return hash;
        }
    }
}
