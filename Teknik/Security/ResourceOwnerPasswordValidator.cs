using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;

namespace Teknik.Security
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly TeknikEntities _dbContext;
        private readonly Config _config;

        public ResourceOwnerPasswordValidator(TeknikEntities dbContext, Config config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // Get the user
            string username = context.UserName;
            User user = UserHelper.GetUser(_dbContext, context.UserName);
            if (user != null)
            {
                bool userValid = UserHelper.UserPasswordCorrect(_dbContext, _config, user, context.Password);
            }
            return null;
        }
    }
}
