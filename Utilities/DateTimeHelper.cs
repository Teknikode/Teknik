using System;

namespace Teknik.Utilities
{
    public static class DateTimeHelper
    {
        public static int GetUnixTimestamp()
        {
            return DateTime.UtcNow.GetUnixTimestamp();
        }

        public static int GetUnixTimestamp(this DateTime dt)
        {
            return (int)(dt.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
