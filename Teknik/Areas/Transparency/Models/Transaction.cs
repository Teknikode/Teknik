using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Transparency.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public double Amount { get; set; }

        public string Currency { get; set; }

        public string Reason { get; set; }

        public DateTime DateSent { get; set; }
    }
}
