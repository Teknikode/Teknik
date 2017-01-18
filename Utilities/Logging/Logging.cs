using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.Logging
{
    public static class Logging
    {
        public static Logger Logger
        {
            get
            {
                return Create();
            }
        }

        public static Logger Create()
        {
            Config curConfig = Config.Load();
            return new Logger(curConfig);
        }
    }
}
