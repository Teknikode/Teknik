using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
    public class AesCounterStream : Stream
    {
        private Stream _Inner;
        private CounterModeCryptoTransform _Cipher;

        /// <summary>
        /// Performs Encryption or Decryption on a stream with the given Key and IV
        /// 
        /// Cipher is AES-256 in CTR mode with no padding
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encrypt"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public AesCounterStream(Stream stream, bool encrypt, byte[] key, byte[] iv)
        {
            _Inner = stream;

            // Create the Aes Cipher
            AesCounterMode aes = new AesCounterMode(iv);
            if (encrypt)
            {
                _Cipher = (CounterModeCryptoTransform)aes.CreateEncryptor(key, iv); // Encrypt
            }
            else
            {
                _Cipher = (CounterModeCryptoTransform)aes.CreateDecryptor(key, iv); // Decrypt
            }

            // Sync the counter
            SyncCounter();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_Inner != null && CanRead)
            {
                int processed = 0;

                // Read the data from the stream
                int bytesRead = await _Inner.ReadAsync(buffer);
                if (bytesRead > 0)
                {
                    // Process the read buffer
                    processed = _Cipher.TransformBlock(buffer.Span, 0, bytesRead);
                }

                // Do we have more?
                if (processed < bytesRead)
                {
                    // Finalize the cipher
                    var finalProcessed = _Cipher.TransformFinalBlock(buffer.Span, processed, bytesRead);
                    if (finalProcessed > 0)
                        processed += finalProcessed;
                }

                return processed;
            }
            return -1;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_Inner != null && CanRead)
            {
                Memory<byte> readBuf = buffer;
                int processed = 0;

                // Read the data from the stream
                int bytesRead = _Inner.Read(readBuf.Span.Slice(offset, count));
                if (bytesRead > 0)
                {
                    // Process the read buffer
                    processed = _Cipher.TransformBlock(readBuf.Span, offset, bytesRead);
                }

                // Do we have more?
                if (processed < bytesRead)
                {
                    // Finalize the cipher
                    var finalProcessed = _Cipher.TransformFinalBlock(readBuf.Span, processed + offset, bytesRead);
                    if (finalProcessed > 0)
                        processed += finalProcessed;
                }

                return processed;
            }
            return -1;
        }

        public override int Read(Span<byte> buffer)
        {
            if (_Inner != null && CanRead)
            {
                int processed = 0;

                // Read the data from the stream
                int bytesRead = _Inner.Read(buffer);
                if (bytesRead > 0)
                {
                    // Process the read buffer
                    processed = _Cipher.TransformBlock(buffer, 0, bytesRead);
                }

                // Do we have more?
                if (processed < bytesRead)
                {
                    // Finalize the cipher
                    var finalProcessed = _Cipher.TransformFinalBlock(buffer, processed, bytesRead);
                    if (finalProcessed > 0)
                        processed += finalProcessed;
                }

                return processed;
            }
            return -1;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_Inner != null && CanWrite)
            {
                // Process the cipher
                Memory<byte> output = buffer;

                // Process the buffer
                int processed = _Cipher.TransformBlock(output.Span, offset, count);

                // Do we have more?
                if (processed < count)
                {
                    // Finalize the cipher
                    var finalProcessed = _Cipher.TransformFinalBlock(output.Span, processed + offset, count);
                    if (finalProcessed > 0)
                        processed += finalProcessed;
                }
                ReadOnlyMemory<byte> readOnlyOutput = buffer;
                _Inner.Write(readOnlyOutput.Span);
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

                    // Sync the counter
                    SyncCounter();
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
                long newPos = _Inner.Seek(offset, origin);

                // Sync the counter
                SyncCounter();

                return newPos;
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

        protected override void Dispose(bool disposing)
        {
            _Inner.Dispose();

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            _Inner.DisposeAsync();

            return base.DisposeAsync();
        }

        private void SyncCounter()
        {
            if (_Cipher != null)
            {
                // Calculate the counter iterations and position needed
                int iterations = (int)Math.Floor(_Inner.Position / (decimal)_Cipher.InputBlockSize);
                int counterPos = (int)(_Inner.Position % _Cipher.InputBlockSize);

                // Are we out of sync with the cipher?
                if (_Cipher.Iterations != iterations + 1 || _Cipher.CounterPosition != counterPos)
                {
                    // Reset the current counter
                    _Cipher.ResetCounter();

                    // Iterate the counter to the current position
                    for (int i = 0; i < iterations; i++)
                    {
                        _Cipher.IncrementCounter();
                    }

                    // Encrypt the counter
                    _Cipher.EncryptCounter();

                    // Set the current position of the counter
                    _Cipher.CounterPosition = counterPos;

                    // Increment the counter for the next time
                    _Cipher.IncrementCounter();
                }
            }
        }
    }
}
