using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CaseSensitiveAttribute : Attribute
    {
        public CaseSensitiveAttribute()
        {
            IsEnabled = true;
        }
        public bool IsEnabled { get; set; }
    }
}