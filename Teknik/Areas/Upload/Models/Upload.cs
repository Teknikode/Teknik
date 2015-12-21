using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Upload.Models
{
    public class Upload
    {
        public int UploadId { get; set; }
        
        public int? UserId { get; set; }

        public User User { get; set; }

        public DateTime DateUploaded { get; set; }

        public string Url { get; set; }

        public int FileSize { get; set; }

        public string Hash { get; set; }

        public string DeleteKey { get; set; }
    }
}