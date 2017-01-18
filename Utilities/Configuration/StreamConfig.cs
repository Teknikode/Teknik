using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class StreamConfig
    {
        public bool Enabled { get; set; }

        public List<string> Sources { get; set; }

        public StreamConfig()
        {
            Enabled = true;
            Sources = new List<string>();
        }
    }
}
