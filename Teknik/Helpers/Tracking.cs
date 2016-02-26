using Piwik.Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Teknik.Configuration;

namespace Teknik.Helpers
{
    public static class Tracking
    {
        public static void TrackPageView(HttpRequestBase request, string title, string sub)
        {
            Config config = Config.Load();
            // Handle Piwik Tracking if enabled
            if (config.PiwikConfig.Enabled)
            {
                try
                {
                    PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId);

                    tracker.setForceVisitDateTime(DateTime.Now);
                    tracker.setUserAgent(request.UserAgent);

                    tracker.setResolution(request.Browser.ScreenPixelsWidth, request.Browser.ScreenPixelsHeight);
                    tracker.setBrowserHasCookies(request.Browser.Cookies);

                    string ipAddress = request.UserHostAddress;

                    tracker.setIp(ipAddress);

                    tracker.setUrl(request.Url.ToString());
                    tracker.setUrlReferrer(request.UrlReferrer.ToString());

                    tracker.doTrackPageView(string.Format("{0} / {1}", sub, title));
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void TrackAction(string userAgent, string url)
        {
            Config config = Config.Load();
            // Handle Piwik Tracking if enabled
            if (config.PiwikConfig.Enabled)
            {
                try
                {
                    PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId);

                    tracker.setUserAgent(userAgent);

                    tracker.doTrackAction(url, PiwikTracker.ActionType.download);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
