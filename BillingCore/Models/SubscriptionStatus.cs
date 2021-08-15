using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public enum SubscriptionStatus
    {
        Incomplete, 
        IncompleteExpired,
        Trialing,
        Active,
        PastDue,
        Canceled,
        Unpaid
    }
}
