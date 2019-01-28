using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Utilities
{
    public static class CookieHelper
    {
        public static string GenerateCookieDomain(string domain, bool local, bool dev)
        {
#if DEBUG
            return null;
#endif
            if (local) // localhost
            {
                return null;
            }
            else if (dev) // dev.example.com
            {
                return string.Format("dev.{0}", domain);
            }
            else // A production instance
            {
                return string.Format(".{0}", domain);
            }
        }
    }
}
