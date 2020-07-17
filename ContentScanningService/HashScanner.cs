using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities.Cryptography;

namespace Teknik.ContentScanningService
{
    public class HashScanner : ContentScanner
    {
        private static readonly HttpClient _client = new HttpClient();
        public HashScanner(Config config) : base(config)
        { }

        public async override Task<ScanResult> ScanFile(Stream stream)
        {
            var result = new ScanResult();
            if (stream != null)
            {
                // Set the start of the stream
                stream.Seek(0, SeekOrigin.Begin);

                if (_config.UploadConfig.HashScanConfig.Authenticate)
                {
                    var byteArray = Encoding.UTF8.GetBytes($"{_config.UploadConfig.HashScanConfig.Username}:{_config.UploadConfig.HashScanConfig.Password}");
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                // compute the hash of the stream
                var hash = Utilities.Cryptography.SHA1.Hash(stream);

                HttpResponseMessage response = await _client.PostAsync(_config.UploadConfig.HashScanConfig.Endpoint, new StringContent(hash));
                HttpContent content = response.Content;

                string resultStr = await content.ReadAsStringAsync();
                if (resultStr == "true")
                {
                    // The hash matched a CP entry, let's return the result as such
                    result.ResultType = ScanResultType.ChildPornography;
                }
                else
                {
                    result.RawResult = resultStr + " | " + hash + _client.DefaultRequestHeaders.Authorization.ToString();
                }

            }
            return result;
        }
    }
}
