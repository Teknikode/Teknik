using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities
{
    public class AESCryptoStream : Stream
    {
        private Stream _Inner;
        private IBufferedCipher _Cipher;

        public AESCryptoStream(Stream stream, bool encrypt, byte[] key, byte[] iv, string mode, string padding, int initCounter)
        {
            _Inner = stream;
            _Cipher = AES.CreateCipher(encrypt, key, iv, mode, padding);

            // Pre-process the cipher to increase the counts
            // There has to be a better way of doing this...
            byte[] buf = new byte[16];
            for (int i = 0; i < initCounter; i++)
            {
                _Cipher.ProcessBytes(buf);
            }
            _Cipher.DoFinal();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_Inner != null && CanRead)
            {
                int bytesRead = 0;
                long startPosition = _Inner.Position;
                int blockSize = _Cipher.GetBlockSize();
                long blockOffset = (startPosition % blockSize);

                // Determine if we are at the start of a block, or not
                if (blockOffset != 0)
                {
                    // We are not a multiple of the block size, so let's backup to get the current block
                    //_Inner.Seek(startPosition - blockOffset, SeekOrigin.Begin);
                }

                // Process the cipher
                int processed = AES.ProcessCipherBlock(_Cipher, _Inner, 0, count, buffer, offset, out bytesRead);

                // Adjust bytes read by the block offset
                //bytesRead -= (int)blockOffset;

                if (processed < bytesRead)
                {
                    // Finalize the cipher
                    processed += AES.FinalizeCipherBlock(_Cipher, buffer, processed + offset);
                }

                return bytesRead;
            }
            return -1;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_Inner != null && CanWrite)
            {
                // Process the cipher
                byte[] output = new byte[count];
                int processed = _Cipher.ProcessBytes(buffer, offset, count, output, 0);

                // Finalize the cipher
                AES.FinalizeCipherBlock(_Cipher, output, processed);
                
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
