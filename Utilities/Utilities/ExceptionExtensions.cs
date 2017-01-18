using System;

namespace Teknik.Utilities
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex)
        {
            return ex.GetFullMessage(false, false);
        }

        public static string GetFullMessage(this Exception ex, bool recursive)
        {
            return ex.GetFullMessage(recursive, false);
        }

        public static string GetFullMessage(this Exception ex, bool recursive, bool stackTrace)
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