using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Teknik.Utilities
{
    public static class ResponseHelper
    {
        public static void StreamToOutput(HttpResponseBase response, bool flush, Stream stream, int length, int chunkSize)
        {
            try
            {
                if (flush)
                {
                    // If the client isn't here, just quit early
                    if (!response.IsClientConnected)
                    {
                        return;
                    }
                    response.Flush();
                }

                int curByte = 0;
                int processedBytes = 0;
                byte[] buffer = new byte[chunkSize];
                int bytesRemaining = length;
                int bytesToRead = chunkSize;
                do
                {
                    if (chunkSize > bytesRemaining)
                    {
                        bytesToRead = bytesRemaining;
                    }

                    processedBytes = stream.Read(buffer, 0, bytesToRead);
                    if (processedBytes > 0)
                    {
                        // If the client isn't here, just quit early
                        if (!response.IsClientConnected)
                        {
                            return;
                        }

                        response.OutputStream.Write(buffer, 0, processedBytes);

                        // Clear the buffer
                        Array.Clear(buffer, 0, chunkSize);

                        // Flush the response
                        if (flush)
                        {
                            // If the client isn't here, just quit early
                            if (!response.IsClientConnected)
                            {
                                return;
                            }
                            //response.OutputStream.Write(buffer, 0, 1);
                            response.Flush();
                        }
                    }
                    curByte += processedBytes;
                    bytesRemaining -= processedBytes;
                }
                while (processedBytes > 0 && bytesRemaining > 0);
            }
            finally
            {
                // dispose of file stream
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        public static void DecryptStreamToOutput(HttpResponseBase response, bool flush, Stream stream, int length, byte[] key, byte[] iv, string mode, string padding, int chunkSize)
        {
            try
            {
                if (flush)
                {
                    response.Flush();
                }
                IBufferedCipher cipher = AES.CreateCipher(false, key, iv, mode, padding);

                int curByte = 0;
                int processedBytes = 0;
                byte[] buffer = new byte[chunkSize];
                int bytesRemaining = length;
                int bytesToRead = chunkSize;
                int bytesRead = 0;
                do
                {
                    if (chunkSize > bytesRemaining)
                    {
                        bytesToRead = bytesRemaining;
                    }
                    processedBytes = AES.ProcessCipherBlock(cipher, stream, bytesToRead, buffer, 0, out bytesRead);
                    if (processedBytes > 0)
                    {
                        response.OutputStream.Write(buffer, 0, processedBytes);
                        if (flush)
                        {
                            response.Flush();
                        }

                        // Clear the buffer
                        Array.Clear(buffer, 0, chunkSize);
                    }
                    curByte += bytesRead;
                    bytesRemaining -= bytesRead;
                }
                while (bytesRemaining > 0);

                // Clear the buffer
                Array.Clear(buffer, 0, chunkSize);

                // Finalize processing of the cipher
                processedBytes = AES.FinalizeCipherBlock(cipher, buffer, 0);
                if (processedBytes > 0)
                {
                    // We have bytes, lets write them to the output
                    response.OutputStream.Write(buffer, 0, processedBytes);
                    if (flush)
                    {
                        response.Flush();
                    }
                }
            }
            catch (HttpException httpEx)
            {
                // If we lost connection, that's fine
                if (httpEx.ErrorCode == -2147023667)
                {
                    // do nothing
                }
                else
                {
                    //throw httpEx;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // dispose of file stream
                stream.Dispose();
            }
        }
    }
}
