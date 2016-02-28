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
        public static void TrackPageView(HttpRequestBase request, Config config, string title)
        {
            // Handle Piwik Tracking if enabled
            if (config.PiwikConfig.Enabled)
            {
                try
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
                    
                    tracker.setUserAgent(request.UserAgent);

                    tracker.setResolution(request.Browser.ScreenPixelsWidth, request.Browser.ScreenPixelsHeight);
                    tracker.setBrowserHasCookies(request.Browser.Cookies);

                    string ipAddress = request.ClientIPFromRequest(true);

                    tracker.setIp(ipAddress);
                    tracker.setTokenAuth(config.PiwikConfig.TokenAuth);

                    tracker.setUrl(request.Url.ToString());
                    if (request.UrlReferrer != null)
                        tracker.setUrlReferrer(request.UrlReferrer.ToString());

                    tracker.setRequestTimeout(5);
                    tracker.doTrackPageView(string.Format("{0}/{1}", sub, title));
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void TrackAction(HttpRequestBase request, string url)
        {
            Config config = Config.Load();
            // Handle Piwik Tracking if enabled
            if (config.PiwikConfig.Enabled)
            {
                try
                {
                    PiwikTracker.URL = config.PiwikConfig.Url;
                    PiwikTracker tracker = new PiwikTracker(config.PiwikConfig.SiteId);

                    tracker.setUserAgent(request.UserAgent);

                    string ipAddress = request.ClientIPFromRequest(true);

                    tracker.setIp(ipAddress);
                    tracker.setTokenAuth(config.PiwikConfig.TokenAuth);

                    tracker.doTrackAction(url, PiwikTracker.ActionType.download);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
