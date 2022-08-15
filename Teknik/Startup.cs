using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teknik.Data;
using Teknik.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Teknik.Configuration;
using Teknik.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using IdentityModel;
using Teknik.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Teknik.Tracking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Teknik.Logging;
using Teknik.Utilities.Routing;
using Teknik.WebCommon.Middleware;
using Teknik.WebCommon;
using Teknik.Areas.Error.Controllers;
using Teknik.Services;
using Teknik.MailService;
using Teknik.Areas.Users.Utility;

namespace Teknik
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            Environment = env;
        }
        
        public IWebHostEnvironment Environment { get; }

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
            var logger = sp.GetService<ILogger<Logger>>();
            var devEnv = config?.DevEnvironment ?? true;
            var defaultConn = config?.DbConnection ?? string.Empty;
            var authority = config?.UserConfig?.IdentityServerConfig?.Authority ?? string.Empty;
            var host = config?.Host ?? string.Empty;
            var apiName = config?.UserConfig?.IdentityServerConfig?.APIName ?? string.Empty;
            var apiSecret = config?.UserConfig?.IdentityServerConfig?.APISecret ?? string.Empty;
            var clientId = config?.UserConfig?.IdentityServerConfig?.ClientId ?? string.Empty;
            var clientSecret = config?.UserConfig?.IdentityServerConfig?.ClientSecret ?? string.Empty;

            if (devEnv)
            {
                Environment.EnvironmentName = Environments.Development;
            }
            else
            {
                Environment.EnvironmentName = Environments.Production;
            }

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (Environment.IsDevelopment()) ? StatusCodes.Status307TemporaryRedirect : StatusCodes.Status308PermanentRedirect;
#if DEBUG
                options.HttpsPort = 8050;
#else
                options.HttpsPort = 443;
#endif
            });

            services.AddControllersWithViews()
                    .AddControllersAsServices()
                    .AddNewtonsoftJson();

            services.AddHostedService<TaskQueueService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<ObjectCache, ObjectCache>(c => new ObjectCache(300));
            services.AddSingleton<IMailService, IMailService>(s => UserHelper.CreateMailService(config, logger));
            services.AddHostedService<CacheCleanerService>();
            services.AddScoped<IErrorController, ErrorController>();

            // Add Tracking Filter scopes
            //services.AddScoped<TrackDownload>();
            //services.AddScoped<TrackLink>();
            //services.AddScoped<TrackPageView>();

            // Create the Database Context
            services.AddDbContext<TeknikEntities>(options => options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(defaultConn), ServiceLifetime.Transient);

            // Cookie Policies
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Domain = CookieHelper.GenerateCookieDomain(host, false, Environment.IsDevelopment());
                options.Cookie.Name = "TeknikWeb";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            // Compression Response
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options => {
                options.Providers.Add<BrotliCompressionProvider>();
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
                options.Cookie.Domain = CookieHelper.GenerateCookieDomain(host, false, Environment.IsDevelopment());
                options.Cookie.Name = "TeknikWebAntiForgery";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            });

            services.AddTransient<CookieEventHandler>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = true;

                    options.ApiName = apiName;
                    options.ApiSecret = apiSecret;

                    options.NameClaimType = "username";
                    options.RoleClaimType = JwtClaimTypes.Role;
                })
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.Cookie.Name = "TeknikWebAuth";
                    options.Cookie.Domain = CookieHelper.GenerateCookieDomain(host, false, Environment.IsDevelopment());

                    options.EventsType = typeof(CookieEventHandler);
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = authority;
                    options.RequireHttpsMetadata = true;

                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
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
                })
                .AddScheme<AuthTokenSchemeOptions, AuthTokenAuthenticationHandler>("AuthToken", "AuthToken", options => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("FullAPI", p =>
                {
                    p.AddAuthenticationSchemes("AuthToken");
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.read");
                    p.RequireScope("teknik-api.write");
                });
                options.AddPolicy("ReadAPI", p =>
                {
                    p.AddAuthenticationSchemes("AuthToken");
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.read");
                });
                options.AddPolicy("WriteAPI", p =>
                {
                    p.AddAuthenticationSchemes("AuthToken");
                    p.AddAuthenticationSchemes("Bearer");
                    p.RequireScope("teknik-api.write");
                });
                options.AddPolicy("AnyAPI", p =>
                {
                    p.AddAuthenticationSchemes("AuthToken");
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
        public void Configure(IApplicationBuilder app, TeknikEntities dbContext, Config config)
        {
            var host = config?.Host ?? string.Empty;
            var shortHost = config?.ShortenerConfig?.ShortenerHost ?? string.Empty;

            // Create and Migrate the database
            dbContext?.Database?.Migrate();

            // Setup static files and cache them client side
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 31536000;
                }
            });

            // Initiate Routing
            app.UseRouting();

            // Setup the HttpContext
            app.UseHttpContextSetup();

            // HttpContext Session
            app.UseSession(new SessionOptions()
            {
                IdleTimeout = TimeSpan.FromMinutes(30),
                Cookie = new CookieBuilder()
                {
                    Domain = CookieHelper.GenerateCookieDomain(host, false, Environment.IsDevelopment()),
                    Name = "TeknikWebSession",
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                }
            });

            // Compress Reponse
            app.UseResponseCompression();

            // Cache Responses
            //app.UseResponseCaching();

            // Force a HTTPS redirection (301)
            app.UseHttpsRedirection();

            // Use Exception Handling
            app.UseErrorHandler();

            // Performance Monitor the entire request
            app.UsePerformanceMonitor();

            // Custom Middleware
            app.UseBlacklist();
            app.UseCORS();
            app.UseCSP();
            app.UseSecurityHeaders();

            // Enable Cookie Policy
            app.UseCookiePolicy();

            // Authorize all the things!
            app.UseAuthentication();
            app.UseAuthorization();

            // And finally, let's use MVC
            app.UseEndpoints(endpoints =>
            {
                endpoints.BuildEndpoints(host, shortHost);
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
