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
        public async static Task StreamToOutput(HttpResponse response, Stream stream, int length, int chunkSize)
        {
            response.RegisterForDisposeAsync(stream);
            var bufferSize = chunkSize;
            if (length < chunkSize)
                bufferSize = length;
            Memory<byte> buffer = new byte[bufferSize];
            try
            {
                int processedBytes;
                do
                {
                    processedBytes = await stream.ReadAsync(buffer);
                    if (processedBytes > 0)
                    {
                        await response.Body.WriteAsync(buffer.Slice(0, processedBytes));

                        await response.Body.FlushAsync();
                    }
                }
                while (processedBytes > 0);
            }
            catch (Exception ex)
            {
                // Don't worry about it.  Just leave
            }
            finally
            {
                await response.Body.FlushAsync();

                // dispose of file stream
                //stream?.Dispose();
            }
        }
    }
}
