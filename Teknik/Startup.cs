using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teknik.Data;
using Teknik.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Teknik.Logging;
using System.IO;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Teknik.Attributes;
using Teknik.Filters;
using Microsoft.Net.Http.Headers;
using Teknik.Areas.Users.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using IdentityModel;
using Teknik.Security;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web;

namespace Teknik
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment = env;
        }
        
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string baseDir = Environment.ContentRootPath;
            string dataDir = Path.Combine(baseDir, "App_Data");
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);

            // Setup IIS
            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
                options.AutomaticAuthentication = false;
            });

            // HTTP Context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Create Configuration Singleton
            services.AddScoped<Config, Config>(opt => Config.Load(dataDir));

            // Build an intermediate service provider
            var sp = services.BuildServiceProvider();

            // Resolve the services from the service provider
            var config = sp.GetService<Config>();

            if (config.DevEnvironment)
            {
                Environment.EnvironmentName = EnvironmentName.Development;
            }
            else
            {
                Environment.EnvironmentName = EnvironmentName.Production;
            }

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (Environment.IsDevelopment()) ? StatusCodes.Status307TemporaryRedirect : StatusCodes.Status308PermanentRedirect;
#if DEBUG
                options.HttpsPort = 5050;
#else
                options.HttpsPort = 443;
#endif
            });

            // Add Tracking Filter scopes
            //services.AddScoped<TrackDownload>();
            //services.AddScoped<TrackLink>();
            //services.AddScoped<TrackPageView>();

            // Create the Database Context
            services.AddDbContext<TeknikEntities>(options => options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(config.DbConnection), ServiceLifetime.Transient);

            // Cookie Policies
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Domain = CookieHelper.GenerateCookieDomain(config.Host, false, Environment.IsDevelopment());
                options.Cookie.Name = "TeknikWeb";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            // Compression Response
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });

            // Sessions
            //services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddSession();

            // Set the anti-forgery cookie name
            services.AddAntiforgery(options =>
            {
                options.Cookie.Domain = CookieHelper.GenerateCookieDomain(config.Host, false, Environment.IsDevelopment());
                options.Cookie.Name = "TeknikWebAntiForgery";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            });

            // Core MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddTransient<CookieEventHandler>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = config.UserConfig.IdentityServerConfig.Authority;
                    options.RequireHttpsMetadata = true;

                    options.ApiName = config.UserConfig.IdentityServerConfig.APIName;
                    options.ApiSecret = config.UserConfig.IdentityServerConfig.APISecret;

                    options.NameClaimType = "username";
                    options.RoleClaimType = JwtClaimTypes.Role;
                })
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                    options.Cookie.Expiration = TimeSpan.FromDays(30);
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.Cookie.Name = "TeknikWebAuth";
                    options.Cookie.Domain = CookieHelper.GenerateCookieDomain(config.Host, false, Environment.IsDevelopment());

                    options.EventsType = typeof(CookieEventHandler);
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = config.UserConfig.IdentityServerConfig.Authority;
                    options.RequireHttpsMetadata = true;

                    options.ClientId = config.UserConfig.IdentityServerConfig.ClientId;
                    options.ClientSecret = config.UserConfig.IdentityServerConfig.ClientSecret;
                    options.ResponseType = "code id_token";

                    // Set the scopes to listen to
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("role");
                    options.Scope.Add("account-info");
                    options.Scope.Add("teknik-api.read");
                    options.Scope.Add("teknik-api.write");
                    options.Scope.Add("offline_access");

                    // Let's clear the claim actions and make our own mappings
                    options.ClaimActions.Clear();
                    options.ClaimActions.MapUniqueJsonKey("sub", "sub");
                    options.ClaimActions.MapUniqueJsonKey("username", "username");
                    options.ClaimActions.MapUniqueJsonKey("role", "role");
                    options.ClaimActions.MapUniqueJsonKey("creation-date", "creation-date");
                    options.ClaimActions.MapUniqueJsonKey("last-seen", "last-seen");
                    options.ClaimActions.MapUniqueJsonKey("account-type", "account-type");
                    options.ClaimActions.MapUniqueJsonKey("account-status", "account-status");
                    options.ClaimActions.MapUniqueJsonKey("recovery-email", "recovery-email");
                    options.ClaimActions.MapUniqueJsonKey("recovery-verified", "recovery-verified");
                    options.ClaimActions.MapUniqueJsonKey("2fa-enabled", "2fa-enabled");
                    options.ClaimActions.MapUniqueJsonKey("pgp-public-key", "pgp-public-key");

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "username",
                        RoleClaimType = JwtClaimTypes.Role
                    };

                    options.Events.OnRemoteFailure = HandleOnRemoteFailure;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("FullAPI", p =>
                {
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.read");
                    p.RequireScope("teknik-api.write");
                });
                options.AddPolicy("ReadAPI", p =>
                {
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.read");
                });
                options.AddPolicy("WriteAPI", p =>
                {
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.write");
                });
                options.AddPolicy("AnyAPI", p =>
                {
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.read", "teknik-api.write");
                });
            });

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = long.MaxValue; // In case of multipart
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, TeknikEntities dbContext, Config config)
        {
            // Create and Migrate the database
            dbContext.Database.Migrate();

            // Initiate Logging
            loggerFactory.AddLogger(config);

            // Setup the HttpContext
            app.UseHttpContextSetup();

            // HttpContext Session
            app.UseSession(new SessionOptions()
            {
                IdleTimeout = TimeSpan.FromMinutes(30),
                Cookie = new CookieBuilder()
                {
                    Domain = CookieHelper.GenerateCookieDomain(config.Host, false, Environment.IsDevelopment()),
                    Name = "TeknikWebSession",
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                }
            });

            // Compress Reponse
            //app.UseResponseCompression();

            // Cache Responses
            //app.UseResponseCaching();

            // Force a HTTPS redirection (301)
            app.UseHttpsRedirection();

            // Use Exception Handling
            app.UseErrorHandler(config);

            // Performance Monitor the entire request
            app.UsePerformanceMonitor();

            // Custom Middleware
            app.UseBlacklist();
            app.UseCORS();
            app.UseCSP();
            app.UseSecurityHeaders();

            // Setup static files and cache them client side
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 31536000;
                }
            });

            // Enable Cookie Policy
            app.UseCookiePolicy();

            // Authorize all the things!
            app.UseAuthentication();

            // And finally, let's use MVC
            app.UseMvc(routes =>
            {
                routes.BuildRoutes(config);
            });
        }

        private async Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            if (context.Failure.Message.Contains("access_denied"))
                context.Response.StatusCode = 403;
            context.HandleResponse();
        }
    }
}
