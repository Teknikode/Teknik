using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.WebCommon.Middleware
{
    public class BlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public BlacklistMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context, Config config)
        {
            // Beggining of Request
            bool blocked = false;
            string blockReason = string.Empty;

            #region Detect Blacklisted IPs
            if (!blocked)
            {
                string IPAddr = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                if (!string.IsNullOrEmpty(IPAddr))
                {
                    StringDictionary badIPs = GetFileData(context, "BlockedIPs", config.IPBlacklistFile);

                    blocked |= (badIPs != null && badIPs.ContainsKey(IPAddr));
                    blockReason = $"This IP address ({IPAddr}) has been blacklisted.  If you feel this is in error, please contact support@teknik.io for assistance.";
                }
            }
            #endregion

            #region Detect Blacklisted Referrers
            if (!blocked)
            {
                string referrer = context.Request.Headers["Referer"].ToString();
                string referrerHost = referrer;
                try
                {
                    var referrerUri = new Uri(referrer);
                    referrerHost = referrerUri.Host;
                } catch
                { }
                if (!string.IsNullOrEmpty(referrer))
                {
                    StringDictionary badReferrers = GetFileData(context, "BlockedReferrers", config.ReferrerBlacklistFile);

                    if (badReferrers != null)
                    {
                        blocked |= badReferrers.ContainsKey(referrer) || badReferrers.ContainsKey(referrerHost);
                        blockReason = $"This referrer ({referrer}) has been blacklisted.  If you feel this is in error, please contact support@teknik.io for assistance.";
                    }
                }
            }
            #endregion

            if (blocked)
            {
                // Clear the response
                context.Response.Clear();

                string jsonResult = JsonConvert.SerializeObject(new { error = new { type = "Blacklist", message = blockReason } });
                await context.Response.WriteAsync(jsonResult);
                return;
            }

            await _next.Invoke(context);

            // End of request
        }

        public StringDictionary GetFileData(HttpContext context, string key, string filePath)
        {
            StringDictionary data;
            if (!_cache.TryGetValue(key, out data))
            {
                data = GetFileLines(filePath);
                _cache.Set(key, data);
            }

            return data;
        }

        public StringDictionary GetFileLines(string configPath)
        {
            StringDictionary retval = new StringDictionary();
            if (File.Exists(configPath))
            {
                using (StreamReader sr = new StreamReader(configPath))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length != 0)
                        {
                            retval.Add(line, null);
                        }
                    }
                }
            }

            return retval;
        }
    }

    public static class BlacklistMiddlewareExtensions
    {
        public static IApplicationBuilder UseBlacklist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BlacklistMiddleware>();
        }
    }
}
