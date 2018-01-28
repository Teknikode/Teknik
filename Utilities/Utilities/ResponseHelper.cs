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
                        response.OutputStream.Write(buffer, 0, processedBytes);

                        // Flush the response
                        if (flush)
                        {
                            response.Flush();
                        }

                        // Clear the buffer
                        Array.Clear(buffer, 0, chunkSize);

                        // decrement the total bytes remaining to process
                        bytesRemaining -= processedBytes;
                    }
                }
                while (processedBytes > 0 && bytesRemaining > 0);
            }
            catch (Exception ex)
            {
                // Don't worry about it.  Just leave
            }
            finally
            {
                // dispose of file stream
                stream?.Dispose();
            }
        }
    }
}
