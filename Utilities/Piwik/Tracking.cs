using Piwik.Tracker;
using System;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Piwik
{
    public static class Tracking
    {
        public static void TrackPageView(HttpRequestBase request, Config config, string title)
        {
            try
            {
                // Follow Do Not Track
                string doNotTrack = request.Headers["DNT"];
                if (string.IsNullOrEmpty(doNotTrack) || doNotTrack != "1")
                {
                    string sub = request.RequestContext.RouteData.Values["sub"].ToString();
                    if (string.IsNullOrEmpty(sub))
                    {
                        sub = request.Url.AbsoluteUri.GetSubdomain();
                    }

                    if (config.DevEnvironment)
                    {
                        sub = "dev - " + sub;
                    }

                    //PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId, config.PiwikConfig.Url);

                    // Get Request Info
                    string ipAddress = request.ClientIPFromRequest(true);
                    tracker.SetIp(ipAddress);
                    tracker.SetTokenAuth(config.PiwikConfig.TokenAuth);
                    tracker.SetUrl(request.Url.ToString());

                    tracker.SetUserAgent(request.UserAgent);

                    // Get browser info
                    tracker.SetResolution(request.Browser.ScreenPixelsWidth, request.Browser.ScreenPixelsHeight);
                    tracker.SetBrowserHasCookies(request.Browser.Cookies);
                    if (!string.IsNullOrEmpty(request.Headers["Accept-Language"]))
                        tracker.SetBrowserLanguage(request.Headers["Accept-Language"]);
                    BrowserPlugins plugins = new BrowserPlugins();
                    plugins.Java = request.Browser.JavaApplets;
                    tracker.SetPlugins(plugins);

                    // Get Referral
                    if (request.UrlReferrer != null)
                        tracker.SetUrlReferrer(request.UrlReferrer.ToString());

                    if (request.Url != null)
                        tracker.SetUrl(request.Url.ToString());

                    // Send the tracking request
                    tracker.DoTrackPageView(string.Format("{0}/{1}", sub, title));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void TrackDownload(bool dnt, int siteId, string siteUrl, string userAgent, string clientIp, string token, string url, string urlReferrer)
        {
            TrackAction(ActionType.Download, dnt, siteId, siteUrl, userAgent, clientIp, token, url, urlReferrer);
        }

        public static void TrackLink(bool dnt, int siteId, string siteUrl, string userAgent, string clientIp, string token, string url, string urlReferrer)
        {
            TrackAction(ActionType.Link, dnt, siteId, siteUrl, userAgent, clientIp, token, url, urlReferrer);
        }

        private static void TrackAction(ActionType type, bool dnt, int siteId, string siteUrl, string userAgent, string clientIp, string token, string url, string urlReferrer)
        {
            try
            {
                // Follow Do Not Track
                if (dnt)
                {
                    PiwikTracker tracker = new PiwikTracker(siteId, siteUrl);

                    tracker.SetUserAgent(userAgent);

                    tracker.SetIp(clientIp);
                    tracker.SetTokenAuth(token);

                    // Get Referral
                    if (!string.IsNullOrEmpty(urlReferrer))
                        tracker.SetUrlReferrer(urlReferrer);

                    if (!string.IsNullOrEmpty(url))
                        tracker.SetUrl(url);

                    tracker.DoTrackAction(url, type);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
