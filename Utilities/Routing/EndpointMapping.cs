using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Utilities.Routing
{
    public class EndpointMapping
    {
        public string Name { get; set; }
        public string Area { get; set; }
        public string Pattern { get; set; }
        public bool? AdjustPattern { get; set; }
        public List<HostType> HostTypes { get; set; }
        public List<string> SubDomains { get; set; }
        public object Defaults { get; set; }
    }
}
