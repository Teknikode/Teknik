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
            var bufferSize = chunkSize;
            if (length < chunkSize)
                bufferSize = length;
            var pooledArray = new PooledArray(bufferSize);

            response.RegisterForDispose(pooledArray);
            try
            {
                int processedBytes;
                do
                {
                    processedBytes = stream.Read(pooledArray.Array, 0, pooledArray.Length);
                    if (processedBytes > 0)
                    {
                        await response.Body.WriteAsync(pooledArray.Array, 0, processedBytes);

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
