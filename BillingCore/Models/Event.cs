using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class Event
    {
        public EventType EventType { get; set; }

        public object Data { get; set; }
    }
}
