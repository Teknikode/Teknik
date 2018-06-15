using System;
using System.IO;

namespace Teknik.Utilities
{
    public static class ByteHelper
    {
        public static void PadToMultipleOf(ref byte[] src, int pad)
        {
            int len = (src.Length + pad - 1) / pad * pad;
            Array.Resize(ref src, len);
        }
    }
}
