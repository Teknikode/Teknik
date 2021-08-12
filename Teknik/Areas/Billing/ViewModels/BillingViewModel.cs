using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class BillingViewModel : ViewModelBase
    {
        public string StripePublishKey { get; set; }
    }
}
