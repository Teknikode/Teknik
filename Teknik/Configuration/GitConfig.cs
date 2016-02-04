using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class GitConfig
    {
        public bool Enabled { get; set; }

        public string Host { get; set; }

        public string AccessToken { get; set; }

        public int SourceId { get; set; }

        public GitConfig()
        {
            Enabled = true;
            Host = string.Empty;
            AccessToken = string.Empty;
            SourceId = 1;
        }
    }
}
