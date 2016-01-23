using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Profile.Models;

namespace Teknik.Areas.Paste.Models
{
    public class Paste
    {
        public int PasteId { get; set; }

        public int? UserId { get; set; }

        public User User { get; set; }

        public DateTime DatePosted { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public string Syntax { get; set; }

        public string HashedPassword { get; set; }

        public string Key { get; set; }

        public int KeySize { get; set; }

        public string IV { get; set; }

        public int BlockSize { get; set; }

        public bool Hide { get; set; }
    }
}
