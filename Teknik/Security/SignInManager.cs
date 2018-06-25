using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class SignInManager : SignInManager<User>
    {
        private readonly UserManager<User> _userManager;
        private readonly TeknikEntities _dbContext;
        private readonly Config _config;

        public SignInManager(
            UserManager<User> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<User> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<User>> logger, 
            IAuthenticationSchemeProvider schemes,
            TeknikEntities dbContext,
            Config config)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _config = config;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            User user = UserHelper.GetUser(_dbContext, userName);
            if (user != null)
            {
                return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
            }
            return SignInResult.Failed;
        }

        public override async Task<SignInResult> PasswordSignInAsync(User user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // Check to see if they are banned
            if (user.AccountStatus == Utilities.AccountStatus.Banned)
            {
                return SignInResult.NotAllowed;
            }

            return await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
    }
}
