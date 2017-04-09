using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Teknik.Utilities
{
    /// <summary>
    /// MVC action result that generates the file content using a delegate that writes the content directly to the output stream.
    /// </summary>
    public class FileGenerateResult : FileResult
    {
        private readonly Action<HttpResponseBase> responseDelegate;

        private readonly bool bufferOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileGeneratingResult" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="content">Delegate with Stream parameter. This is the stream to which content should be written.</param>
        /// <param name="bufferOutput">use output buffering. Set to false for large files to prevent OutOfMemoryException.</param>
        public FileGenerateResult(string fileName, string contentType, Action<HttpResponseBase> response, bool bufferOutput)
            : base(contentType)
        {
            if (response == null)
                throw new ArgumentNullException("content");

            this.responseDelegate = response;
            this.bufferOutput = bufferOutput;
        }

        /// <summary>
        /// Writes the file to the response.
        /// </summary>
        /// <param name="response">The response object.</param>
        protected override void WriteFile(System.Web.HttpResponseBase response)
        {
            response.Buffer = bufferOutput;
            response.BufferOutput = bufferOutput;
            responseDelegate(response);
        }
    }
}
