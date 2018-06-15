using System.Text;

namespace Teknik.Utilities
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string AddStringAtInterval(this string value, int interval, string insertStr)
        {
            if (interval <= 0 || value.Length < interval)
                return value;

            StringBuilder sb = new StringBuilder();

            int finalIndex = 0;
            for (int i = 0; i < value.Length; i = i + interval)
            {
                sb.Append(value.Substring(i, interval));
                sb.Append(insertStr);
                finalIndex = i;
            }

            if (finalIndex + interval != value.Length)
            {
                sb.Append(value.Substring(finalIndex, value.Length - finalIndex));
            }

            return sb.ToString();
        }
    }
}