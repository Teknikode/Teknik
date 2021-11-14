using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.ViewModels;

namespace Teknik.Areas.Billing.ViewModels
{
    public class CancelSubscriptionViewModel : ViewModelBase
    {
        public string ProductName { get; set; }
    }
}
