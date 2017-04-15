using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
    public class SHA384
    {
        public static byte[] Hash(string key, string value)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] data = Encoding.UTF8.GetBytes(value);

            var cipher = new System.Security.Cryptography.HMACSHA384(keyBytes);
            byte[] result = cipher.ComputeHash(data);

            return result;
        }
    }
}
