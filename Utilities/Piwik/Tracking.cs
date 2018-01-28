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

                    PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId);

                    // Get Request Info
                    string ipAddress = request.ClientIPFromRequest(true);
                    tracker.setIp(ipAddress);
                    tracker.setTokenAuth(config.PiwikConfig.TokenAuth);
                    tracker.setUrl(request.Url.ToString());

                    tracker.setUserAgent(request.UserAgent);

                    // Get browser info
                    tracker.setResolution(request.Browser.ScreenPixelsWidth, request.Browser.ScreenPixelsHeight);
                    tracker.setBrowserHasCookies(request.Browser.Cookies);
                    if (!string.IsNullOrEmpty(request.Headers["Accept-Language"]))
                        tracker.setBrowserLanguage(request.Headers["Accept-Language"]);
                    BrowserPlugins plugins = new BrowserPlugins();
                    plugins.java = request.Browser.JavaApplets;
                    tracker.setPlugins(plugins);

                    // Get Referral
                    if (request.UrlReferrer != null)
                        tracker.setUrlReferrer(request.UrlReferrer.ToString());

                    // Send the tracking request
                    tracker.doTrackPageView(string.Format("{0}/{1}", sub, title));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void TrackDownload(HttpRequestBase request, Config config, string url)
        {
            TrackAction(request, config, url, PiwikTracker.ActionType.download);
        }

        public static void TrackLink(HttpRequestBase request, Config config, string url)
        {
            TrackAction(request, config, url, PiwikTracker.ActionType.link);
        }

        private static void TrackAction(HttpRequestBase request, Config config, string url, PiwikTracker.ActionType type)
        {
            try
            {
                // Follow Do Not Track
                string doNotTrack = request.Headers["DNT"];
                if (string.IsNullOrEmpty(doNotTrack) || doNotTrack != "1")
                {
                    PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId);

                    tracker.setUserAgent(request.UserAgent);

                    string ipAddress = request.ClientIPFromRequest(true);

                    tracker.setIp(ipAddress);
                    tracker.setTokenAuth(config.PiwikConfig.TokenAuth);

                    // Get Referral
                    if (request.UrlReferrer != null)
                        tracker.setUrlReferrer(request.UrlReferrer.ToString());

                    tracker.doTrackAction(url, type);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
