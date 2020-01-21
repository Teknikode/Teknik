using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.Areas.Users.Models
{
    public class UploadSettings
    {
        [Column("Encrypt")]
        public bool Encrypt { get; set; }

        [Column("ExpirationLength")]
        public int ExpirationLength { get; set; }

        [Column("ExpirationUnit")]
        public ExpirationUnit ExpirationUnit { get; set; }

        [Column("MaxUploadStorage")]
        public long? MaxUploadStorage { get; set; }

        public UploadSettings()
        {
            Encrypt = false;
            ExpirationLength = 1;
            ExpirationUnit = ExpirationUnit.Never;
            MaxUploadStorage = null;
        }
    }
}
