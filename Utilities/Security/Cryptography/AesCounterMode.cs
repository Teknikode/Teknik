using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Security.Cryptography
{
    public class AesCounterMode : SymmetricAlgorithm
    {
        private const int _blockSize = 16;
        private readonly byte[] _counter;
        private readonly AesManaged _aes;

        public AesCounterMode(byte[] counter)
        {
            if (counter == null) throw new ArgumentNullException("counter");
            if (counter.Length != _blockSize)
                throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
                    counter.Length, _blockSize));

            _aes = new AesManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };

            _counter = counter;
        }

        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
        {
            return new CounterModeCryptoTransform(_aes, key, iv, _counter);
        }

        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
        {
            return new CounterModeCryptoTransform(_aes, key, iv, _counter);
        }

        public override void GenerateKey()
        {
            _aes.GenerateKey();
        }

        public override void GenerateIV()
        {
            // IV not needed in Counter Mode
        }
    }

    public class CounterModeCryptoTransform : ICryptoTransform
    {
        private readonly byte[] _iv;
        private readonly byte[] _counter;
        private readonly ICryptoTransform _counterEncryptor;
        private readonly Queue<byte> _xorMask = new Queue<byte>();
        private readonly SymmetricAlgorithm _symmetricAlgorithm;

        public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] iv, byte[] counter)
        {
            if (symmetricAlgorithm == null) throw new ArgumentNullException("symmetricAlgorithm");
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("counter");
            if (iv.Length != symmetricAlgorithm.BlockSize / 8)
                throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
                    iv.Length, symmetricAlgorithm.BlockSize / 8));

            _symmetricAlgorithm = symmetricAlgorithm;
            _counter = counter;
            _iv = iv;
            
            _counterEncryptor = symmetricAlgorithm.CreateEncryptor(key, iv);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var output = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (var i = 0; i < inputCount; i++)
            {
                if (NeedMoreXorMaskBytes()) EncryptCounterThenIncrement();

                var mask = _xorMask.Dequeue();
                outputBuffer[outputOffset + i] = (byte)(mask ^ inputBuffer[inputOffset + i]);
            }

            return inputCount;
        }

        private bool NeedMoreXorMaskBytes()
        {
            return _xorMask.Count == 0;
        }

        private void EncryptCounterThenIncrement()
        {
            var counterModeBlock = new byte[_symmetricAlgorithm.BlockSize / 8];

            _counterEncryptor.TransformBlock(_counter, 0, _counter.Length, counterModeBlock, 0);
            IncrementCounter();

            foreach (var b in counterModeBlock)
            {
                _xorMask.Enqueue(b);
            }
        }

        private void IncrementCounter()
        {
            int j = _counter.Length;
            while (--j >= 0 && ++_counter[j] == 0)
            {
            }
            //for (var i = _counter.Length - 1; i >= 0; i--)
            //{
            //    if (++_counter[i] != 0)
            //        break;
            //}
        }

        public int InputBlockSize { get { return _symmetricAlgorithm.BlockSize / 8; } }
        public int OutputBlockSize { get { return _symmetricAlgorithm.BlockSize / 8; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return false; } }

        public void Dispose()
        {
        }
    }
}
