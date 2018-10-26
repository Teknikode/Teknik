using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class UserSettings
    {
        [Column("About")]
        public string About { get; set; }

        [Column("Website")]
        public string Website { get; set; }

        [Column("Quote")]
        public string Quote { get; set; }

        public UserSettings()
        {
            About = string.Empty;
            Website = string.Empty;
            Quote = string.Empty;
        }
    }
}
