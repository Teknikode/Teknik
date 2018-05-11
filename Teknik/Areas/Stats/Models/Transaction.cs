using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.Areas.Stats.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public CurrencyType Currency { get; set; }

        public string Reason { get; set; }

        public DateTime DateSent { get; set; }
    }
}
