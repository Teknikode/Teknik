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

                    PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId, config.PiwikConfig.Url);

                    // Set Request Info
                    tracker.setIp(clientIp);
                    tracker.setTokenAuth(config.PiwikConfig.TokenAuth);

                    tracker.setUserAgent(userAgent);

                    // Set browser info
                    tracker.setResolution(pixelWidth, pixelHeight);
                    tracker.setBrowserHasCookies(hasCookies);
                    if (!string.IsNullOrEmpty(acceptLang))
                        tracker.setBrowserLanguage(acceptLang);
                    tracker.setPlugins(new BrowserPlugins {java = hasJava});

                    // Get Referral
                    if (!string.IsNullOrEmpty(urlReferrer))
                        tracker.setUrlReferrer(urlReferrer);

                    if (!string.IsNullOrEmpty(url))
                        tracker.setUrl(url);

                    // Send the tracking request
                    tracker.doTrackPageView(string.Format("{0}/{1}", sub, title));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void TrackDownload(string userAgent, string clientIp, string url, string urlReferrer)
        {
            TrackAction(PiwikTracker.ActionType.download, userAgent, clientIp, url, urlReferrer);
        }

        public static void TrackLink(string userAgent, string clientIp, string url, string urlReferrer)
        {
            TrackAction(PiwikTracker.ActionType.link, userAgent, clientIp, url, urlReferrer);
        }

        private static void TrackAction(PiwikTracker.ActionType type, string userAgent, string clientIp, string url, string urlReferrer)
        {
            try
            {
                Config config = Config.Load();

                if (config.PiwikConfig.Enabled)
                {
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId, config.PiwikConfig.Url);

                    tracker.setUserAgent(userAgent);

                    tracker.setIp(clientIp);
                    tracker.setTokenAuth(config.PiwikConfig.TokenAuth);

                    // Get Referral
                    if (!string.IsNullOrEmpty(urlReferrer))
                        tracker.setUrlReferrer(urlReferrer);

                    if (!string.IsNullOrEmpty(url))
                        tracker.setUrl(url);

                    tracker.doTrackAction(url, type);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
