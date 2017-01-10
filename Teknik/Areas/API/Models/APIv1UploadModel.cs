using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Areas.API.Models
{
    public class APIv1UploadModel : APIv1BaseModel
    {
        public HttpPostedFileWrapper file { get; set; }

        public string contentType { get; set; }

        public bool encrypt { get; set; }

        public bool saveKey { get; set; }

        public string key { get; set; }

        public int keySize { get; set; }

        public string iv { get; set; }

        public int blockSize { get; set; }

        public bool genDeletionKey { get; set; }

        public APIv1UploadModel()
        {
            file = null;
            contentType = null;
            encrypt = true;
            saveKey = true;
            key = null;
            keySize = 0;
            iv = null;
            blockSize = 0;
            genDeletionKey = false;
        }
    }
}