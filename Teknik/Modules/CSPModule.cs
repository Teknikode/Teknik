using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Modules
{
    public class CSPModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += delegate (object sender, EventArgs args)
            {
                HttpContext requestContext = ((HttpApplication)sender).Context;
                if (!requestContext.Request.IsLocal)
                {
                    // Default to nothing allowed
                    string allowedDomain = "'none'";

                    // Allow this domain
                    string host = requestContext.Request.Url.Host;

                    if (!string.IsNullOrEmpty(host))
                    {
                        string domain = host.GetDomain();
                        string sub = host.GetSubdomain();

                        allowedDomain = string.Format("{0}.{1} {1}", (string.IsNullOrEmpty(sub) ? "*" : sub), domain);
                    }

                    // If a CDN is enabled, then add the cdn host
                    Config config = Config.Load();
                    if (config.UseCdn)
                    {
                        allowedDomain += " " + config.CdnHost;
                    }

                    requestContext.Response.AppendHeader("Content-Security-Policy", string.Format("default-src 'none'; script-src blob: 'unsafe-eval' 'nonce-{1}' {0}; style-src 'unsafe-inline' {0}; img-src data: *; font-src data: {0}; connect-src wss: blob: data: {0}; media-src *; worker-src blob: mediastream: {0}; form-action {0}; base-uri {0}; frame-ancestors {0};", allowedDomain, requestContext.Items[Constants.NONCE_KEY]));
                }
            };
        }
    }
}
