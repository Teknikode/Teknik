using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Stats.ViewModels
{
    public class BillViewModel : TransactionViewModel
    {
        public string Recipient { get; set; }
    }
}
