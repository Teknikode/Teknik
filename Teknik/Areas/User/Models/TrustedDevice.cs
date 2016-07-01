using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Attributes;

namespace Teknik.Areas.Users.Models
{
    public class TrustedDevice
    {
        public int TrustedDeviceId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Name { get; set; }

        [CaseSensitive]
        public string Token { get; set; }

        public DateTime DateSeen { get; set; }
    }
}