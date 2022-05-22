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
            int processedBytes = 0;
            var bufferSize = chunkSize;
            if (length < chunkSize)
                bufferSize = length;
            Memory<byte> buffer = new byte[bufferSize];
            try
            {
                do
                {
                    processedBytes = await stream.ReadAsync(buffer);
                    if (processedBytes > 0)
                    {
                        await response.Body.WriteAsync(buffer.Slice(0, processedBytes));

                        // Flush the response
                        if (flush)
                        {
                            await response.Body.FlushAsync();
                        }
                    }
                }
                while (processedBytes > 0);
            }
            catch (Exception)
            {
                // Don't worry about it.  Just leave
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
