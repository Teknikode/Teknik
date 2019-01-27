using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class UserFeature
    {
        public int UserFeatureId { get; set; }

        public string FeatureId { get; set; }

        public virtual Feature Feature { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
