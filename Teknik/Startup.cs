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
using Teknik.Areas.Accounts;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Teknik.Security;
using Teknik.Attributes;
using Teknik.Filters;
using Microsoft.Net.Http.Headers;
using Teknik.Areas.Users.Models;

namespace Teknik
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
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

            // Add Tracking Filter scopes
            //services.AddScoped<TrackDownload>();
            //services.AddScoped<TrackLink>();
            //services.AddScoped<TrackPageView>();

            // Create the Database Context
            services.AddDbContext<TeknikEntities>(options => options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(Configuration.GetConnectionString("TeknikEntities")), ServiceLifetime.Transient);

            // Cookie Policies
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            // Add Identity User
            services.AddIdentity<User, Role>()
                .AddUserStore<UserStore>()
                .AddRoleStore<RoleStore>()
                .AddDefaultTokenProviders();

            services.AddTransient<IUserStore<User>, UserStore>();
            services.AddTransient<IRoleStore<Role>, RoleStore>();
            services.AddTransient<IPasswordHasher<User>, PasswordHasher>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "TeknikAuth";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            // Identity Server
            services.AddIdentityServer(options => 
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                
                if (Environment.IsDevelopment())
                {
                    options.UserInteraction.LoginUrl = new PathString("/Login?sub=user");
                    options.UserInteraction.ConsentUrl = new PathString("/Consent?sub=user");
                }
                else
                {
                    options.UserInteraction.LoginUrl = new PathString("/User/User/Login");
                    options.UserInteraction.ConsentUrl = new PathString("/User/User/Consent");
                }

                // Setup Auth Cookies
                options.Authentication.CheckSessionCookieName = "TeknikAuth";
            })
                .AddDeveloperSigningCredential()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
                .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
                .AddInMemoryClients(IdentityServerConfig.GetClients())
                .AddAspNetIdentity<User>();

            // Setup Authentication Service
            services.AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ApiName = "api";
                })
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.ApiName = "api";

                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(10);
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
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddSession();

            // Set the anti-forgery cookie name
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "TeknikAntiForgery";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            });

            // Core MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                    Domain = null,
                    Name = "TeknikSession",
                    SecurePolicy = CookieSecurePolicy.Always,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                }
            });

            // Use Exception Handling
            app.UseErrorHandler(config);

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                //app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            // Performance Monitor the entire request
            app.UsePerformanceMonitor();

            // Custom Middleware
            app.UseBlacklist();
            app.UseCORS();
            app.UseCSP();
            app.UseSecurityHeaders();

            // Cache Responses
            app.UseResponseCaching();

            // Force a HTTPS redirection (301)
            app.UseHttpsRedirection();

            // Setup static files anc cache them client side
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
            app.UseIdentityServer();

            // And finally, let's use MVC
            app.UseMvc(routes =>
            {
                routes.BuildRoutes(config);
            });
        }
    }
}
