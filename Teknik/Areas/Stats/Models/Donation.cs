using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Stats.Models
{
    public class Donation : Transaction
    {
        public string Sender { get; set; }
    }
}
