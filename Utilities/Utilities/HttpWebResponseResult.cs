using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Teknik.Utilities
{
    /// <summary>
    /// Result for relaying an HttpWebResponse
    /// </summary>
    public class HttpWebResponseResult : ActionResult
    {
        private readonly HttpWebResponse _response;
        private readonly ActionResult _innerResult;

        /// <summary>
        /// Relays an HttpWebResponse as verbatim as possible.
        /// </summary>
        /// <param name="responseToRelay">The HTTP response to relay</param>
        public HttpWebResponseResult(HttpWebResponse responseToRelay)
        {
            if (responseToRelay == null)
            {
                throw new ArgumentNullException("response");
            }

            _response = responseToRelay;

            Stream contentStream;
            if (responseToRelay.ContentEncoding.Contains("gzip"))
            {
                contentStream = new GZipStream(responseToRelay.GetResponseStream(), CompressionMode.Decompress);
            }
            else if (responseToRelay.ContentEncoding.Contains("deflate"))
            {
                contentStream = new DeflateStream(responseToRelay.GetResponseStream(), CompressionMode.Decompress);
            }
            else
            {
                contentStream = responseToRelay.GetResponseStream();
            }


            if (string.IsNullOrEmpty(responseToRelay.CharacterSet))
            {
                // File result
                _innerResult = new FileStreamResult(contentStream, responseToRelay.ContentType);
            }
            else
            {
                // Text result
                var contentResult = new ContentResult();
                contentResult = new ContentResult();
                contentResult.Content = new StreamReader(contentStream).ReadToEnd();
                _innerResult = contentResult;
            }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var clientResponse = context.HttpContext.Response;
            clientResponse.StatusCode = (int)_response.StatusCode;

            foreach (var headerKey in _response.Headers.AllKeys)
            {
                switch (headerKey)
                {
                    case "Content-Length":
                    case "Transfer-Encoding":
                    case "Content-Encoding":
                        // Handled by IIS
                        break;

                    default:
                        clientResponse.AddHeader(headerKey, _response.Headers[headerKey]);
                        break;
                }
            }

            _innerResult.ExecuteResult(context);
        }
    }

    public enum ResultType
    {
        Passthrough,
        Json
    }
}