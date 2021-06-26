using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Teknik.Utilities.Routing
{
    public static class EndpointHelper
    {
        public static void BuildEndpoints(this IEndpointRouteBuilder endpoints, string fullHost, string shortHost)
        {
            // Get the endpoint mappings
            var mappings = GetEndpointMappings();

            // Create a subdomain endpoint for each mapping
            foreach (var mapping in mappings)
            {
                var domains = GetDomainsFromHostTypes(fullHost, shortHost, mapping.HostTypes);

                var defaultObj = new ExpandoObject();
                if (mapping.Defaults != null)
                {
                    var defaults = mapping.Defaults as JObject;
                    foreach (var defaultVal in defaults)
                    {
                        defaultObj.TryAdd(defaultVal.Key, defaultVal.Value);
                    }
                }
                defaultObj.TryAdd("area", mapping.Area);

                endpoints.MapSubdomainEndpoint(
                  name: mapping.Name,
                  domains: domains,
                  subDomains: mapping.SubDomains,
                  pattern: mapping.Pattern,
                  area: mapping.Area,
                  defaults: defaultObj,
                  adjustPattern: mapping.AdjustPattern ?? true
                );
            }
        }

        public static EndpointMapping GetEndpointMapping(string name)
        {
            var mappings = GetEndpointMappings();
            return mappings.FirstOrDefault(m => m.Name == name);
        }

        public static List<EndpointMapping> GetEndpointMappings()
        {
            string dataDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            string file = Path.Combine(dataDir, Constants.ENDPOINT_MAPPING_PATH);
            if (File.Exists(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return (List<EndpointMapping>)serializer.Deserialize(jsonTextReader, typeof(List<EndpointMapping>));
                }
            }
            return new List<EndpointMapping>();
        }

        private static List<string> GetDomainsFromHostTypes(string fullHost, string shortHost, List<HostType> hostTypes)
        {
            var domains = new List<string>();

            if (hostTypes != null)
            {
                foreach (var hostType in hostTypes)
                {
                    switch (hostType)
                    {
                        case HostType.Full:
                            if (!string.IsNullOrEmpty(fullHost) &&
                                !domains.Contains(fullHost))
                                domains.Add(fullHost);
                            break;
                        case HostType.Short:
                            if (!string.IsNullOrEmpty(shortHost) &&
                                !domains.Contains(shortHost))
                                domains.Add(shortHost);
                            break;
                    }
                }
            }

            return domains;
        }
    }
}
