using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public class PooledArray : IDisposable
    {
        private static ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create();

        public byte[] Array { get; private set; }

        public readonly int Length;

        public PooledArray(int size)
        {
            Array = _arrayPool.Rent(size);
            Length = size;
        }

        public PooledArray(byte[] array)
        {
            Length = array.Length;
            Array = _arrayPool.Rent(array.Length);
            array.CopyTo(Array, 0);
        }

        public PooledArray(PooledArray array)
        {
            Length = array.Length;
            Array = _arrayPool.Rent(array.Length);
            array.CopyTo(Array);
        }

        public void CopyTo(byte[] destination)
        {
            System.Array.Copy(Array, destination, Length);
        }

        public byte[] ToArray()
        {
            return Array.Take(Length).ToArray();
        }

        public void Dispose()
        {
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _arrayPool.Return(Array);
            }
        }
    }
}
