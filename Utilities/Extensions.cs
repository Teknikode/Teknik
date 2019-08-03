using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Teknik.Utilities
{
    public static class Extensions
    {
        public static string GetDescription<T>(this T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public static string GetDisplayName<T>(this T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DisplayNameAttribute[] attributes = fi.GetCustomAttributes(typeof(DisplayNameAttribute), false) as DisplayNameAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().DisplayName;
            }

            return value.ToString();
        }

        public static bool IsReadOnly<T>(this T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            ReadOnlyAttribute[] attributes = fi.GetCustomAttributes(typeof(ReadOnlyAttribute), false) as ReadOnlyAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().IsReadOnly;
            }

            return false;
        }
    }
}
