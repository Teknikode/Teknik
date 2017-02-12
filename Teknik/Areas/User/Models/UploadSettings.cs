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
        [Key]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual SecuritySettings SecuritySettings { get; set; }

        public virtual BlogSettings BlogSettings { get; set; }

        public virtual UserSettings UserSettings { get; set; }

        public bool Encrypt { get; set; }

        public UploadSettings()
        {
            Encrypt = false;
        }
    }
}
