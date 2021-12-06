using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;

namespace Teknik.Security
{
    public class AuthTokenAuthenticationHandler : AuthenticationHandler<AuthTokenSchemeOptions>
    {
        private readonly TeknikEntities _db;
        private readonly Config _config;
        public AuthTokenAuthenticationHandler(
               IOptionsMonitor<AuthTokenSchemeOptions> options,
               ILoggerFactory logger,
               UrlEncoder encoder,
               ISystemClock clock,
               TeknikEntities dbContext,
               Config config)
               : base(options, logger, encoder, clock)
        {
            _db = dbContext;
            _config = config;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // validation comes in here
            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                return AuthenticateResult.Fail("Header Not Found.");
            }

            var header = Request.Headers[HeaderNames.Authorization].ToString();
            var tokenMatch = Regex.Match(header, $"AuthToken (?<token>.*)");

            if (tokenMatch.Success)
            {
                // the token is captured in this group
                // as declared in the Regex
                var token = tokenMatch.Groups["token"].Value;

                IdentityUserInfo userInfo = null;
                try
                {
                    userInfo = await IdentityHelper.GetIdentityUserInfoByToken(_config, token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception Occured while Deserializing: " + ex);
                    return AuthenticateResult.Fail("TokenParseException");
                }

                // success branch
                // generate authTicket
                // authenticate the request
                if (userInfo != null)
                {
                    // create claims array from the model
                    var claims = new[] 
                    {
                        new Claim(ClaimTypes.Name, userInfo.Username),
                        new Claim("scope", "teknik-api.read"),
                        new Claim("scope", "teknik-api.write")
                    };

                    // generate claimsIdentity on the name of the class
                    var claimsIdentity = new ClaimsIdentity(claims, nameof(AuthTokenAuthenticationHandler));

                    // generate AuthenticationTicket from the Identity
                    // and current authentication scheme
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);

                    // pass on the ticket to the middleware
                    return AuthenticateResult.Success(ticket);
                }
            }

            // failure branch
            // return failure
            // with an optional message
            return AuthenticateResult.Fail("Invalid Authentication Token");
        }
    }
}
