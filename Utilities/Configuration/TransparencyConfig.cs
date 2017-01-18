using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class TransparencyConfig
    {
        public bool Enabled { get; set; }

        public string CanaryPath { get; set; }

        public TransparencyConfig()
        {
            Enabled = false;
            CanaryPath = string.Empty;
        }
    }
}
