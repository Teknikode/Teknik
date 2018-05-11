using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Stats.Models
{
    public class Takedown
    {
        public int TakedownId { get; set; }

        public string Requester { get; set; }

        public string RequesterContact { get; set; }

        public string Reason { get; set; }

        public string ActionTaken { get; set; }

        public DateTime DateRequested { get; set; }

        public DateTime DateActionTaken { get; set; }

        public virtual ICollection<Upload.Models.Upload> Attachments { get; set; }
    }
}
