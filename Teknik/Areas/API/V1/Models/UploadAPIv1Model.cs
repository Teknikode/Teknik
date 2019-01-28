using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Utilities;

namespace Teknik.Areas.API.V1.Models
{
    public class UploadAPIv1Model : BaseAPIv1Model
    {
        public IFormFile file { get; set; }

        public string contentType { get; set; }

        public bool encrypt { get; set; }

        public int expirationLength { get; set; }

        public ExpirationUnit expirationUnit { get; set; }

        public bool saveKey { get; set; }

        public string key { get; set; }

        public int keySize { get; set; }

        public string iv { get; set; }

        public int blockSize { get; set; }

        public bool genDeletionKey { get; set; }

        public UploadAPIv1Model()
        {
            file = null;
            contentType = null;
            encrypt = true;
            expirationLength = 1;
            expirationUnit = ExpirationUnit.Never;
            saveKey = true;
            key = null;
            keySize = 0;
            iv = null;
            blockSize = 0;
            genDeletionKey = false;
        }
    }
}