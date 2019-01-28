using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;

namespace Teknik.Areas.Users.Utility
{
    public static class UserExtensions
    {
        public static bool HasFeatures(this User user, params string[] features)
        {
            if (user.Features.Any())
            {
                return user.Features.Where(f => features.Contains(f.FeatureId)).FirstOrDefault() != null;
            }
            return false;
        }
        public static bool HasFeature(this User user, string feature)
        {
            if (user.Features.Any())
            {
                return user.Features.Where(f => f.FeatureId == feature).FirstOrDefault() != null;
            }
            return false;
        }
    }
}
