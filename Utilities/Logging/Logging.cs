using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Logging
{
    public static class Logging
    {
        private static Logger m_Logger { get; set; }
        public static Logger Logger
        {
            get
            {
                if (m_Logger == null)
                {
                    Create();
                }
                return m_Logger;
            }
        }

        public static void Create()
        {
            m_Logger = new Logger();
        }
    }
}
