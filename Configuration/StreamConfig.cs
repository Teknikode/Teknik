using System.Collections.Generic;

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
