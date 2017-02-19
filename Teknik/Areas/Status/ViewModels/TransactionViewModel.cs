using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Status.Models;
using Teknik.Utilities;
using Teknik.ViewModels;

namespace Teknik.Areas.Status.ViewModels
{
    public class TransactionViewModel : ViewModelBase
    {
        public decimal Amount { get; set; }

        public CurrencyType Currency { get; set; }

        public string Reason { get; set; }

        public DateTime DateSent { get; set; }
    }
}