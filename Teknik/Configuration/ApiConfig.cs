using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Configuration
{
    public class ApiConfig
    {
        public int Version { get; set; }

        public ApiConfig()
        {
            Version = 1;
        }
    }
}
