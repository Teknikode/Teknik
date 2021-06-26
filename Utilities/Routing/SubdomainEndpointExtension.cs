using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using System.Dynamic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Teknik.Utilities.Routing
{
    public static class SubdomainEndpointExtension
    {
        public static void MapSubdomainEndpoint(this IEndpointRouteBuilder routeBuilder, string name, string area, List<string> subDomains, List<string> domains, string pattern)
        {
            MapSubdomainEndpoint(routeBuilder, name, area, subDomains, domains, pattern, null, new { }, true);
        }

        public static void MapSubdomainEndpoint(this IEndpointRouteBuilder routeBuilder, string name, string area, List<string> subDomains, List<string> domains, string pattern, object defaults)
        {
            MapSubdomainEndpoint(routeBuilder, name, area, subDomains, domains, pattern, defaults, new { }, true);
        }

        public static void MapSubdomainEndpoint(this IEndpointRouteBuilder routeBuilder, string name, string area, List<string> subDomains, List<string> domains, string pattern, object defaults, bool adjustPattern)
        {
            MapSubdomainEndpoint(routeBuilder, name, area, subDomains, domains, pattern, defaults, new { }, adjustPattern);
        }

        public static void MapSubdomainEndpoint(this IEndpointRouteBuilder routeBuilder, string name, string area, List<string> subDomains, List<string> domains, string pattern, object defaults, object dataTokens, bool adjustPattern)
        {

            var env = routeBuilder.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            if (env.EnvironmentName == Environments.Development)
            {
                // Prepend the area to the pattern
                if (adjustPattern)
                {
                    pattern = $"{area}/{pattern}";
                }

                routeBuilder.MapAreaControllerRoute(
                    name: name,
                    areaName: area,
                    pattern,
                    defaults: defaults,
                    dataTokens: dataTokens);
            }
            else
            {
                routeBuilder.MapAreaControllerRoute(
                    name: name,
                    areaName: area,
                    pattern.Trim('/'),
                    defaults: defaults,
                    dataTokens: dataTokens)
                .RequireHost(BuildHosts(subDomains, domains));
            }
        }

        private static string[] BuildHosts(List<string> subdomains, List<string> hosts)
        {
            var fullHosts = new List<string>();
            foreach (var sub in subdomains)
            {
                foreach (var host in hosts)
                {
                    fullHosts.Add($"{sub}.{host}");
                }
            }
            return fullHosts.ToArray();
        }
    }
}