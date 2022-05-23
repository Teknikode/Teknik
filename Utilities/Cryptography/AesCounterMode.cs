using System;
using System.Security.Cryptography;

namespace Teknik.Utilities.Cryptography
{
    public class AesCounterMode : SymmetricAlgorithm
    {
        // Internal Variables
        private const int _BlockSize = 16;
        private readonly byte[] _InitialCounter;
        private readonly AesManaged _Algo;

        public AesCounterMode() : this(new byte[_BlockSize]) { }

        public AesCounterMode(byte[] initialCounter)
        {
            if (initialCounter == null) throw new ArgumentNullException("counter");
            if (initialCounter.Length != _BlockSize)
                throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
                    initialCounter.Length, _BlockSize));

            // Generate a new instance of the Aes Algorithm in ECB mode with no padding
            _Algo = new AesManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };

            // Set the internal variables
            _InitialCounter = initialCounter;
        }

        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
        {
            return new CounterModeCryptoTransform(_Algo, key, iv, _InitialCounter);
        }

        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
        {
            return new CounterModeCryptoTransform(_Algo, key, iv, _InitialCounter);
        }

        public override void GenerateKey()
        {
            _Algo.GenerateKey();
        }

        public override void GenerateIV()
        {
            _Algo.GenerateIV();
        }

        protected override void Dispose(bool disposed)
        {
            _Algo.Dispose();
        }
    }

    public class CounterModeCryptoTransform : ICryptoTransform
    {
        private readonly int _BlockSize;
        private readonly Memory<byte> _IV;
        private readonly byte[] _Counter;
        private readonly byte[] _EncryptedCounter;
        private readonly ICryptoTransform _CounterEncryptor;

        // Stateful Fields

        private int _Iterations;
        public int Iterations
        {
            get
            {
                return _Iterations;
            }
        }

        private int _CounterPosition;
        public int CounterPosition
        {
            get
            {
                return _CounterPosition;
            }
            set
            {
                if (value >= 0 && value < _EncryptedCounter.Length)
                {
                    _CounterPosition = value;
                }
            }
        }

        public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] iv, byte[] initialCounter)
        {
            if (symmetricAlgorithm == null) throw new ArgumentNullException("symmetricAlgorithm");
            if (key == null) throw new ArgumentNullException("key");
            if (iv == null) throw new ArgumentNullException("iv");
            if (initialCounter == null) throw new ArgumentNullException("counter");

            // Check lengths
            if (initialCounter.Length != symmetricAlgorithm.BlockSize / 8)
                throw new ArgumentException(String.Format("Counter size must be same as block size (actual: {0}, expected: {1})",
                    initialCounter.Length, symmetricAlgorithm.BlockSize / 8));

            _BlockSize = symmetricAlgorithm.BlockSize;

            // Initialize the encrypted counter
            _EncryptedCounter = new byte[_BlockSize / 8];

            _IV = iv;

            _Counter = initialCounter;
            
            _CounterEncryptor = symmetricAlgorithm.CreateEncryptor(key, iv);

            // Initialize State
            _CounterPosition = 0;
            _Iterations = 0;

            // Encrypt the counter
            EncryptCounter();

            // Initial Increment
            IncrementCounter();
        }

        public int TransformFinalBlock(Span<byte> inputBuffer, int inputOffset, int inputCount)
        {
            return TransformBlock(inputBuffer, inputOffset, inputCount);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var output = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        public int TransformBlock(Span<byte> inputBuffer, int inputOffset, int inputCount)
        {
            for (var i = 0; i < inputCount; i++)
            {
                // Encrypt the counter if we have reached the end, or 
                if (_CounterPosition >= _EncryptedCounter.Length)
                {
                    //Reset current counter position
                    _CounterPosition = 0;

                    // Encrypt the counter
                    EncryptCounter();

                    // Increment the counter for the next batch
                    IncrementCounter();
                }

                // XOR the encrypted counter with the input plain text
                inputBuffer[inputOffset + i] = (byte)(_EncryptedCounter[_CounterPosition] ^ inputBuffer[inputOffset + i]);

                // Move the counter position
                _CounterPosition++;
            }

            return inputCount;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            ReadOnlySpan<byte> input = inputBuffer;
            Span<byte> output = outputBuffer;
            return TransformBlock(input, inputOffset, inputCount, output, outputOffset);
        }

        public int TransformBlock(ReadOnlySpan<byte> inputBuffer, int inputOffset, int inputCount, Span<byte> outputBuffer, int outputOffset)
        {
            for (var i = 0; i < inputCount; i++)
            {
                // Encrypt the counter if we have reached the end, or 
                if (_CounterPosition >= _EncryptedCounter.Length)
                {
                    //Reset current counter position
                    _CounterPosition = 0;

                    // Encrypt the counter
                    EncryptCounter();

                    // Increment the counter for the next batch
                    IncrementCounter();
                }

                // XOR the encrypted counter with the input plain text
                outputBuffer[outputOffset + i] = (byte)(_EncryptedCounter[_CounterPosition] ^ inputBuffer[inputOffset + i]);

                // Move the counter position
                _CounterPosition++;
            }

            return inputCount;
        }

        public void EncryptCounter()
        {
            // Encrypt the current counter to the encrypted counter
            _CounterEncryptor.TransformBlock(_Counter, 0, _Counter.Length, _EncryptedCounter, 0);
        }

        public void ResetCounter()
        {
            _IV.CopyTo(_Counter);
            _Iterations = 0;
        }

        public void IncrementCounter()
        {
            int j = _Counter.Length;
            while (--j >= 0 && ++_Counter[j] == 0)
            {
            }
            _Iterations++;
        }

        public int InputBlockSize { get { return _BlockSize / 8; } }
        public int OutputBlockSize { get { return _BlockSize / 8; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return false; } }

        public void Dispose()
        {
            _CounterEncryptor.Dispose();
        }
    }
}
