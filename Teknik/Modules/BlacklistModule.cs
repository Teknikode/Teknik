using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Caching;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using Teknik.Areas.Error.Controllers;
using Teknik.Configuration;

namespace Teknik.Modules
{
    public class BlacklistModule : IHttpModule
    {

        private EventHandler onBeginRequest;

        public BlacklistModule()
        {
            onBeginRequest = new EventHandler(this.HandleBeginRequest);
        }

        void IHttpModule.Dispose()
        {
        }

        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += onBeginRequest;
        }

        #region Referrer Info
        private const string BLOCKEDREFERRERKEY = "BlockedReferrer";
        private static string referrerFileName = null;
        private static object referrerFileNameObj = new object();

        public static string GetReferrerFilePath()
        {
            if (referrerFileName != null)
                return referrerFileName;
            lock (referrerFileNameObj)
            {
                if (referrerFileName == null)
                {
                    Config config = Config.Load();
                    referrerFileName = config.ReferrerBlacklistFile;
                }
            }

            return referrerFileName;
        }
        #endregion

        #region IP Info
        private const string BLOCKEDIPKEY = "BlockedIP";
        private static string ipFileName = null;
        private static object ipFileNameObj = new object();

        public static string GetIPFilePath()
        {
            if (ipFileName != null)
                return ipFileName;
            lock (ipFileNameObj)
            {
                if (ipFileName == null)
                {
                    Config config = Config.Load();
                    ipFileName = config.IPBlacklistFile;
                }
            }

            return ipFileName;
        }
        #endregion

        public static StringDictionary GetFileData(HttpContext context, string key, Func<string> fn)
        {
            StringDictionary data = (StringDictionary)context.Cache[key];
            if (data == null)
            {
                string filePath = fn();
                data = GetFileLines(filePath);
                context.Cache.Insert(key, data, new CacheDependency(filePath));
            }

            return data;
        }

        public static StringDictionary GetFileLines(string configPath)
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

        private void HandleBeginRequest(object sender, EventArgs evargs)
        {
            HttpApplication app = sender as HttpApplication;

            if (app != null)
            {
                bool blocked = false;
                string blockReason = string.Empty;

                #region Detect Blacklisted IPs
                if (!blocked)
                {
                    string IPAddr = app.Context.Request.ServerVariables["REMOTE_ADDR"];
                    if (!string.IsNullOrEmpty(IPAddr))
                    {
                        StringDictionary badIPs = GetFileData(app.Context, BLOCKEDIPKEY, GetIPFilePath);

                        blocked |= (badIPs != null && badIPs.ContainsKey(IPAddr));
                        blockReason = $"This IP address ({IPAddr}) has been blacklisted.  If you feel this is in error, please contact support@teknik.io for assistance.";
                    }
                }
                #endregion

                #region Detect Blacklisted Referrers
                if (!blocked)
                {
                    string referrer = app.Context.Request.UrlReferrer?.Host;
                    if (!string.IsNullOrEmpty(referrer))
                    {
                        StringDictionary badReferrers = GetFileData(app.Context, BLOCKEDREFERRERKEY, GetReferrerFilePath);

                        blocked |= (badReferrers != null && badReferrers.ContainsKey(referrer));
                        blockReason = $"This referrer ({referrer}) has been blacklisted.  If you feel this is in error, please contact support@teknik.io for assistance.";
                    }
                }
                #endregion

                if (blocked)
                {
                    // Clear the response
                    app.Context.Response.Clear();

                    RouteData routeData = new RouteData();
                    routeData.DataTokens.Add("namespaces", new[] { typeof(ErrorController).Namespace });
                    routeData.DataTokens.Add("area", "Error");
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("scheme", "https");
                    routeData.Values.Add("action", "Http403");

                    // Clear the error on server.
                    app.Context.Server.ClearError();

                    // Avoid IIS7 getting in the middle
                    app.Context.Response.TrySkipIisCustomErrors = true;

                    string jsonResult = Json.Encode(new { error = new { type = "Blacklist", message = blockReason } });
                    app.Context.Response.Write(jsonResult);
                    app.Context.Response.End();
                    return;
                }
            }
        }
    }
}
