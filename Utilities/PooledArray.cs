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

        public PooledArray(int size)
        {
            Array = _arrayPool.Rent(size);
        }

        public void Dispose()
        {
            _arrayPool.Return(Array);
        }
    }
}
