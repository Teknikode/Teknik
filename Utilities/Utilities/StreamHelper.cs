using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Teknik.Security.Cryptography;

namespace Teknik.Utilities
{
    public class AESCryptoStream : Stream
    {
        private Stream _Inner;
        //private IBufferedCipher _Cipher;
        private ICryptoTransform _Cipher;

        public AESCryptoStream(Stream stream, bool encrypt, byte[] key, byte[] iv, string mode, string padding, int initCounter)
        {
            _Inner = stream;

            // Create initial counter value from IV
            byte[] counter = new byte[iv.Length];
            iv.CopyTo(counter, 0);

            // Increment the counter depending on the init counter
            for (int i = 0; i < initCounter; i++)
            {
                int j = counter.Length;
                while (--j >= 0 && ++counter[j] == 0) { }
            }

            // Create the Aes Cipher
            AesCounterMode aes = new AesCounterMode(counter);
            if (encrypt)
            {
                _Cipher = aes.CreateEncryptor(key, iv); // Encrypt
            }
            else
            {
                _Cipher = aes.CreateDecryptor(key, iv); // Decrypt
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_Inner != null && CanRead)
            {
                int bytesRead = 0;
                int totalBytesRead = 0;
                int bytesProcessed = 0;
                long startPosition = _Inner.Position;
                int blockSize = _Cipher.InputBlockSize;
                long byteOffset = (startPosition % blockSize);
                int initialOffset = (int)byteOffset;
                int blockOffset = (byteOffset == 0) ? 0 : 1;
                int blocksToProcess = (int)Math.Ceiling(count / (double)blockSize) + blockOffset;

                // Determine if we are at the start of a block, or not
                if (byteOffset != 0)
                {
                    // We are not a multiple of the block size, so let's backup to get the current block
                    _Inner.Seek(startPosition - byteOffset, SeekOrigin.Begin);
                }

                // Initialize buffers
                byte[] readBuf = new byte[blockSize];
                byte[] outBuf = new byte[blockSize];

                // Iterate through each block of the ammount we want to read
                for (int i = 0; i < blocksToProcess; i++)
                {
                    // Read the next block of data
                    totalBytesRead += bytesRead = _Inner.Read(readBuf, 0, blockSize);
                    if (bytesRead > 0)
                    {
                        // process the cipher for the read block and add it to the output
                        int processed = _Cipher.TransformBlock(readBuf, 0, bytesRead, outBuf, 0);

                        // copy the values to the output
                        outBuf.Skip(initialOffset).ToArray().CopyTo(buffer, bytesProcessed + offset);

                        // Reset initial offset and calibrate
                        bytesProcessed += processed - initialOffset;

                        // Reset initial offset
                        initialOffset = 0;
                    }

                    // Clear the read buffer
                    Array.Clear(readBuf, 0, blockSize);
                }

                // Adjust bytes read by the block offset
                totalBytesRead -= (int)byteOffset;
                bytesProcessed -= (int)byteOffset;

                if (bytesProcessed < count)
                {
                    // Finalize the cipher
                    byte[] finalBuf = _Cipher.TransformFinalBlock(readBuf, bytesProcessed, bytesRead);
                    finalBuf.Take(count - bytesProcessed).ToArray().CopyTo(buffer, bytesProcessed);
                    bytesProcessed += count - bytesProcessed;
                }

                return bytesProcessed;
            }
            return -1;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_Inner != null && CanWrite)
            {
                // Process the cipher
                byte[] output = new byte[count];
                //int processed = _Cipher.ProcessBytes(buffer, offset, count, output, 0);

                // Finalize the cipher
                //AES.FinalizeCipherBlock(_Cipher, output, processed);
                
                _Inner.Write(output, 0, count);
            }
        }

        public override bool CanRead
        {
            get
            {
                if (_Inner != null)
                {
                    return _Inner.CanRead;
                }
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (_Inner != null)
                {
                    return _Inner.CanSeek;
                }
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_Inner != null)
                {
                    return _Inner.CanWrite;
                }
                return false;
            }
        }

        public override long Length
        {
            get
            {
                if (_Inner != null)
                {
                    return _Inner.Length;
                }
                return -1;
            }
        }

        public override long Position
        {
            get
            {
                if (_Inner != null)
                {
                    return _Inner.Position;
                }
                return -1;
            }

            set
            {
                if (_Inner != null)
                {
                    _Inner.Position = value;
                }
            }
        }

        public override void Flush()
        {
            if (_Inner != null)
            {
                _Inner.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_Inner != null)
            {
                return _Inner.Seek(offset, origin);
            }
            return -1;
        }

        public override void SetLength(long value)
        {
            if (_Inner != null)
            {
                _Inner.SetLength(value);
            }
        }
    }
}
