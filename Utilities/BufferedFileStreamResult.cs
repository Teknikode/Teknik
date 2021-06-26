using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Teknik.Utilities
{
    /// <summary>
    /// MVC action result that generates the file content using a delegate that writes the content directly to the output stream.
    /// </summary>
    public class BufferedFileStreamResult : FileResult
    {
        private readonly Func<HttpResponse, Task> responseDelegate;

        private readonly bool bufferOutput;

        public BufferedFileStreamResult(string contentType, Func<HttpResponse, Task> response, bool bufferOutput) : base (contentType)
        {
            if (response == null)
                throw new ArgumentNullException("content");

            this.responseDelegate = response;
            this.bufferOutput = bufferOutput;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (!bufferOutput)
            {
                var bufferingFeature = context.HttpContext.Features.Get<IHttpResponseBodyFeature>();
                bufferingFeature?.DisableBuffering();
            }

            return responseDelegate(context.HttpContext.Response);
        }
    }
}
