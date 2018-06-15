using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Piwik
{
    public class VisitorData
    {
        public DateTime Date { get; set; }
        public int UniqueVisitors { get; set; }
        public int Visits { get; set; }
        public int VisitsConverted { get; set; }
        public int Actions { get; set; }
        public decimal ActionsPerVisit { get; set; }
        public int MaxActions { get; set; }
        public int BounceCount { get; set; }
        public string BounceRate { get; set; }
        public int AverageTimeOnSite { get; set; }
        public int VisitLengthTotal { get; set; }

        public VisitorData()
        {
            Date = DateTime.Now;
            UniqueVisitors = 0;
            Visits = 0;
            VisitsConverted = 0;
            Actions = 0;
            ActionsPerVisit = 0;
            MaxActions = 0;
            BounceCount = 0;
            BounceRate = string.Empty;
            AverageTimeOnSite = 0;
            VisitLengthTotal = 0;
        }
    }
}
