using System;

namespace Teknik.Utilities
{
    public static class ByteExtensions
    {
        public static string ToHex(this byte[] bytes)
        {
            string hashString = string.Empty;
            foreach (byte x in bytes)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}
