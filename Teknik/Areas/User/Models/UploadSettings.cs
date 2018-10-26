using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class UploadSettings
    {
        [Column("Encrypt")]
        public bool Encrypt { get; set; }

        public UploadSettings()
        {
            Encrypt = false;
        }
    }
}
