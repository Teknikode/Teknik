using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;

namespace Teknik.Areas.Upload.Models
{
    public class Upload
    {
        public int UploadId { get; set; }
        
        public int? UserId { get; set; }

        public User User { get; set; }

        public DateTime DateUploaded { get; set; }

        public string Url { get; set; }

        public string FileName { get; set; }

        public int ContentLength { get; set; }

        public string ContentType { get; set; }

        public string Key { get; set; }

        public string IV { get; set; }

        public int KeySize { get; set; }

        public int BlockSize { get; set; }

        public string DeleteKey { get; set; }

        public int Downloads { get; set; }
    }
}