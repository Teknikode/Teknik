using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Modules
{
    public class CORSModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += delegate(object sender, EventArgs args)
            {
                HttpContext requestContext = ((HttpApplication)sender).Context;
                Config config = Config.Load();

                // Allow this domain, or everything if local
                string origin = (requestContext.Request.IsLocal) ? "*" : requestContext.Request.Headers.Get("Origin");
                string domain = (string.IsNullOrEmpty(origin)) ? string.Empty : origin.GetDomain();

                if (string.IsNullOrEmpty(origin))
                {
                    string sub = requestContext.Request.Url.Host.GetSubdomain();
                    origin = (string.IsNullOrEmpty(sub)) ? config.Host : sub + "." + config.Host;
                }
                else
                {
                    if (domain != config.Host)
                    {
                        string sub = origin.GetSubdomain();
                        origin = (string.IsNullOrEmpty(sub)) ? config.Host : sub + "." + config.Host;
                    }
                }

                requestContext.Response.AppendHeader("Access-Control-Allow-Origin", origin);
            };
        }
    }
}
