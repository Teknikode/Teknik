using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Profile.Models
{
    public class UserSettings
    {
        [Key]
        public int UserId { get; set; }

        public string About { get; set; }

        public string Website { get; set; }

        public string Quote { get; set; }

        public UserSettings()
        {
            About = string.Empty;
            Website = string.Empty;
            Quote = string.Empty;
        }
    }
}
