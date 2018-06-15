using Microsoft.AspNetCore.Http;
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
        public async static Task StreamToOutput(HttpResponse response, bool flush, Stream stream, int length, int chunkSize)
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
                        await response.Body.WriteAsync(buffer, 0, processedBytes);

                        // Flush the response
                        if (flush)
                        {
                            await response.Body.FlushAsync();
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
                await response.Body.FlushAsync();
            }
            finally
            {
                await response.Body.FlushAsync();
                // dispose of file stream
                stream?.Dispose();
            }
        }
    }
}
