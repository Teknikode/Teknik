using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Attributes;

namespace Teknik.Areas.Billing.Models
{
    public class Customer
    {
        [CaseSensitive]
        public string CustomerId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
