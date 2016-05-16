using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Helpers
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex, bool recursive = false, bool stackTrace = false)
        {
            string message = ex.Message;
            if (recursive && ex.InnerException != null)
            {
                message += " | Inner Exception: " + ex.InnerException.GetFullMessage(recursive, stackTrace);
            }
            if (stackTrace && !string.IsNullOrEmpty(ex.StackTrace))
            {
                message += " | Stack Trace: " + ex.StackTrace;
            }
            return message;
        }
    }
}