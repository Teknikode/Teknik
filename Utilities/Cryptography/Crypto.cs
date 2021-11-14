using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
    public static class Crypto
    {
        public static string GenerateKey(int keySize)
        {
            return StringHelper.RandomString(keySize / 8);
        }

        public static string GenerateIV(int ivSize)
        {
            return StringHelper.RandomString(ivSize / 16);
        }

        public static string HashPassword(string key, string password)
        {
            return SHA384.Hash(key, password).ToHex();
        }
    }
}
