using Microsoft.AspNetCore.Http;
using Piwik.Tracker;
using System;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Tracking
{
    public static class Tracking
    {
        public static void TrackPageView(string token, int siteId, string apiUrl, string title, string sub, string clientIp, string url, string urlReferrer, string userAgent, int pixelWidth, int pixelHeight, bool hasCookies, string acceptLang, bool hasJava, bool isDev)
        {
            if (isDev)
            {
                sub = "dev - " + sub;
            }

            PiwikTracker tracker = new PiwikTracker(siteId, apiUrl);

            // Set Request Info
            tracker.SetIp(clientIp);
            tracker.SetTokenAuth(token);

            tracker.SetUserAgent(userAgent);

            // Set browser info
            tracker.SetResolution(pixelWidth, pixelHeight);
            tracker.SetBrowserHasCookies(hasCookies);
            if (!string.IsNullOrEmpty(acceptLang))
                tracker.SetBrowserLanguage(acceptLang);
            tracker.SetPlugins(new BrowserPlugins { Java = hasJava });

            // Get Referral
            if (!string.IsNullOrEmpty(urlReferrer))
                tracker.SetUrlReferrer(urlReferrer);

            if (!string.IsNullOrEmpty(url))
                tracker.SetUrl(url);

            // Send the tracking request
            tracker.DoTrackPageView(string.Format("{0}/{1}", sub, title));
        }

        public static void TrackDownload(string token, int siteId, string apiUrl, string userAgent, string clientIp, string url, string urlReferrer)
        {
            TrackAction(token, siteId, apiUrl, ActionType.Download, userAgent, clientIp, url, urlReferrer);
        }

        public static void TrackLink(string token, int siteId, string apiUrl, string userAgent, string clientIp, string url, string urlReferrer)
        {
            TrackAction(token, siteId, apiUrl, ActionType.Link, userAgent, clientIp, url, urlReferrer);
        }

        private static void TrackAction(string token, int siteId, string apiUrl, ActionType type, string userAgent, string clientIp, string url, string urlReferrer)
        {
            PiwikTracker tracker = new PiwikTracker(siteId, apiUrl);

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
}
