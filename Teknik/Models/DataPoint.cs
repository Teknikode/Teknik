using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Models
{
    public class DataPoint
    {
        public long X { get; set; }

        public long Y { get; set; }

        public DataPoint() : this(0, 0)
        {
        }

        public DataPoint(long x, long y)
        {
            X = x;
            Y = y;
        }
    }
}