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
            services.AddScoped<TrackDownload>();
            services.AddScoped<TrackLink>();
            services.AddScoped<TrackPageView>();

            // Create the Database Context
            services.AddDbContext<TeknikEntities>(options => options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(Configuration.GetConnectionString("TeknikEntities")), ServiceLifetime.Transient);

            // Cookie Policies
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Setup Authentication Service
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => 
                {
                    options.Cookie.Domain = null;
                    options.Cookie.Name = "TeknikAuthCore";
                    options.LoginPath = "/User/User/Login";
                    options.LogoutPath = "/User/User/Logout";
                    options.EventsType = typeof(TeknikCookieAuthenticationEvents);
                });
            services.AddScoped<TeknikCookieAuthenticationEvents>();

            // Compression Response
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>();
            });

            // Sessions
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddSession();

            // Core MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //services.AddIdentityServer()
            //    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
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
                    Name = "TeknikSession"
                }
            });

            // Use Exception Handling
            app.UseErrorHandler();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                //app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseHsts();
            }

            // Performance Monitor the entire request
            app.UsePerformanceMonitor();

            // Custom Middleware
            app.UseBlacklist();
            app.UseCORS();
            app.UseCSP();

            // Cache Responses
            app.UseResponseCaching();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.BuildRoutes(config);
            });
        }
    }
}
