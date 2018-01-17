using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Attributes;

namespace Teknik.Areas.Users.Models
{
    public class InviteCode
    {
        public int InviteCodeId { get; set; }

        public bool Active { get; set; }

        [CaseSensitive]
        public string Code { get; set; }

        public virtual User Owner { get; set; }

        public virtual User ClaimedUser { get; set; }

        public InviteCode()
        {
            Active = false;
        }
    }
}
