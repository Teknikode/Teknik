using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Profile.Models
{
    public class UploadSettings
    {
        [Key]
        public int UserId { get; set; }

        public bool SaveKey { get; set; }

        public bool ServerSideEncrypt { get; set; }

        public UploadSettings()
        {
            SaveKey = false;
            ServerSideEncrypt = false;
        }
    }
}
