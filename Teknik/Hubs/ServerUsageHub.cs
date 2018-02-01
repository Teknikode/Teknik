using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using Teknik.Configuration;
using Teknik.SignalR;

namespace Teknik.Hubs
{
    [HubName("serverUsage")]
    public class ServerUsageHub : Hub
    {
        private readonly ServerUsageTicker _serverUsageTicker;

        public ServerUsageHub() : this(ServerUsageTicker.Instance) { }

        public ServerUsageHub(ServerUsageTicker serverUsageTicker)
        {
            _serverUsageTicker = serverUsageTicker;
        }

        public class ServerUsage
        {
            [JsonProperty("cpu")]
            public CPUUsage CPU { get; set; }
            [JsonProperty("memory")]
            public MemoryUsage Memory { get; set; }
            [JsonProperty("network")]
            public NetworkUsage Network { get; set; }

            public ServerUsage()
            {
                CPU = new CPUUsage();
                Memory = new MemoryUsage();
                Network = new NetworkUsage();
            }
        }

        public class CPUUsage
        {
            [JsonProperty("total")]
            public float Total { get; set; }

            public CPUUsage()
            {
                Total = 0;
            }
        }

        public class MemoryUsage
        {
            [JsonProperty("total")]
            public float Total { get; set; }
            [JsonProperty("available")]
            public float Available { get; set; }
            [JsonProperty("used")]
            public float Used { get; set; }
            [JsonProperty("websiteUsed")]
            public float WebsiteUsed { get; set; }
            [JsonProperty("databaseUsed")]
            public float DatabaseUsed { get; set; }

            public MemoryUsage()
            {
                Total = 0;
                Available = 0;
                Used = 0;
                WebsiteUsed = 0;
                DatabaseUsed = 0;
            }
        }

        public class NetworkUsage
        {
            [JsonProperty("sent")]
            public float Sent { get; set; }
            [JsonProperty("received")]
            public float Received { get; set; }

            public NetworkUsage()
            {
                Sent = 0;
                Received = 0;
            }
        }
    }
}
