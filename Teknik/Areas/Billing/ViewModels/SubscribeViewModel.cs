using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.BillingCore.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class SubscribeViewModel : ViewModelBase
    {
        public Subscription Subscription { get; set; }
        public Price Price { get; set; }
    }
}
