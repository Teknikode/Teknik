﻿using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Teknik.IdentityServer.Models;
using Teknik.Utilities;

namespace Teknik.IdentityServer
{
    public class TeknikProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeknikProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();

            claims.AddRange(user.ToClaims());

            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null && user.AccountStatus == Utilities.AccountStatus.Active;
        }
    }
}
