using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore
{
    public interface IBillingService
    {
        public bool AddSubscription();
        public bool EditSubscription();
        public bool RemoveSubscription();
        public void GetSubscription();
        public void SyncSubscriptions();
    }
}
