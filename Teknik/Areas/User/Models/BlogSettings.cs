using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class BlogSettings
    {
        [Column("Title")]
        public string Title { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        public BlogSettings()
        {
            Title = string.Empty;
            Description = string.Empty;
        }
    }
}
