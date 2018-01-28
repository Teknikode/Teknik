using Piwik.Tracker;
using System;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Piwik
{
    public static class Tracking
    {
        public static void TrackPageView(string title, string sub, string clientIp, string url, string urlReferrer, string userAgent, int pixelWidth, int pixelHeight, bool hasCookies, string acceptLang, bool hasJava)
        {
            try
            {
                Config config = Config.Load();

                if (config.PiwikConfig.Enabled)
                {
                    if (config.DevEnvironment)
                    {
                        sub = "dev - " + sub;
                    }

                    //PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId, config.PiwikConfig.Url);

                    // Set Request Info
                    tracker.SetIp(clientIp);
                    tracker.SetTokenAuth(config.PiwikConfig.TokenAuth);

                    tracker.SetUserAgent(userAgent);

                    // Set browser info
                    tracker.SetResolution(pixelWidth, pixelHeight);
                    tracker.SetBrowserHasCookies(hasCookies);
                    if (!string.IsNullOrEmpty(acceptLang))
                        tracker.SetBrowserLanguage(acceptLang);
                    tracker.SetPlugins(new BrowserPlugins {Java = hasJava});

                    // Get Referral
                    if (!string.IsNullOrEmpty(urlReferrer))
                        tracker.SetUrlReferrer(urlReferrer);

                    if (!string.IsNullOrEmpty(url))
                        tracker.SetUrl(url);

                    // Send the tracking request
                    tracker.DoTrackPageView(string.Format("{0}/{1}", sub, title));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void TrackDownload(string userAgent, string clientIp, string url, string urlReferrer)
        {
            TrackAction(ActionType.Download, userAgent, clientIp, url, urlReferrer);
        }

        public static void TrackLink(string userAgent, string clientIp, string url, string urlReferrer)
        {
            TrackAction(ActionType.Link, userAgent, clientIp, url, urlReferrer);
        }

        private static void TrackAction(ActionType type, string userAgent, string clientIp, string url, string urlReferrer)
        {
            try
            {
                Config config = Config.Load();

                if (config.PiwikConfig.Enabled)
                {
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId, config.PiwikConfig.Url);

                    tracker.SetUserAgent(userAgent);

                    tracker.SetIp(clientIp);
                    tracker.SetTokenAuth(config.PiwikConfig.TokenAuth);

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
