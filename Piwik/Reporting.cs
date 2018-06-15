using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.Piwik
{
    public static class Reporting
    {
        public static List<VisitorData> GetVisitSummaryByDays(Config config, int days)
        {
            List<VisitorData> visitorData = new List<VisitorData>();
            try
            {
                if (config.PiwikConfig.Enabled)
                {
                    //PiwikAnalytics.URL = config.PiwikConfig.API;
                    //VisitsSummary visitSummary = new VisitsSummary();
                    //visitSummary.setTokenAuth(config.PiwikConfig.TokenAuth);

                    //Hashtable results = visitSummary.Get(
                    //     config.PiwikConfig.SiteId,
                    //     PiwikPeriod.DAY,
                    //     RelativeRangeDate.LAST(days)
                    //     );

                    //foreach (string period in results.Keys)
                    //{
                    //    // Create a new object to return
                    //    VisitorData data = new VisitorData();

                    //    // Set Period Date
                    //    DateTime date = new DateTime(1900, 1, 1);
                    //    DateTime.TryParse(period, out date);
                    //    data.Date = date;

                    //    // Pull Out Data
                    //    if (results[period].GetType() == typeof(Hashtable))
                    //    {
                    //        Hashtable result = (Hashtable) results[period];

                    //        int UniqueVisitors = 0;
                    //        int.TryParse(result["nb_uniq_visitors"].ToString(), out UniqueVisitors);
                    //        data.UniqueVisitors = UniqueVisitors;

                    //        int visits = 0;
                    //        int.TryParse(result[VisitsSummary.NB_VISITS].ToString(), out visits);
                    //        data.Visits = visits;

                    //        int VisitsConverted = 0;
                    //        int.TryParse(result[VisitsSummary.NB_VISITS_CONVERTED].ToString(), out VisitsConverted);
                    //        data.VisitsConverted = VisitsConverted;

                    //        int Actions = 0;
                    //        int.TryParse(result[VisitsSummary.NB_ACTIONS].ToString(), out Actions);
                    //        data.Actions = Actions;

                    //        decimal ActionsPerVisit = 0;
                    //        decimal.TryParse(result[VisitsSummary.NB_ACTIONS_PER_VISIT].ToString(), out ActionsPerVisit);
                    //        data.ActionsPerVisit = ActionsPerVisit;

                    //        int MaxActions = 0;
                    //        int.TryParse(result[VisitsSummary.MAX_ACTIONS].ToString(), out MaxActions);
                    //        data.MaxActions = MaxActions;

                    //        int BounceCount = 0;
                    //        int.TryParse(result[VisitsSummary.BOUNCE_COUNT].ToString(), out BounceCount);
                    //        data.BounceCount = BounceCount;

                    //        data.BounceRate = result[VisitsSummary.BOUNCE_RATE].ToString();

                    //        int AverageTimeOnSite = 0;
                    //        int.TryParse(result[VisitsSummary.AVG_TIME_ON_SITE].ToString(), out AverageTimeOnSite);
                    //        data.AverageTimeOnSite = AverageTimeOnSite;

                    //        int VisitLengthTotal = 0;
                    //        int.TryParse(result[VisitsSummary.SUM_VISIT_LENGTH].ToString(), out VisitLengthTotal);
                    //        data.VisitLengthTotal = VisitLengthTotal;
                    //    }

                    //    visitorData.Add(data);
                    //}
                }
            }
            catch (Exception ex)
            {
                
            }

            return visitorData;
        }
    }
}
