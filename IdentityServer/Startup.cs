using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Teknik.Configuration;
using Teknik.IdentityServer.Configuration;
using Teknik.IdentityServer.Security;
using Teknik.IdentityServer.Middleware;
using Teknik.Logging;
using Microsoft.AspNetCore.Authorization;
using Teknik.IdentityServer.Models;
using IdentityServer4.Services;

namespace Teknik.IdentityServer
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

        public void ConfigureServices(IServiceCollection services)
        {
            string dataDir = Configuration["ConfigDirectory"];
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Create Configuration Singleton
            services.AddScoped<Config, Config>(opt => Config.Load(dataDir));

            // Build an intermediate service provider
            var sp = services.BuildServiceProvider();

            // Resolve the services from the service provider
            var config = sp.GetService<Config>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "TeknikAuth";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
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
                options.Cookie.Name = "TeknikAuthAntiForgery";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<ApplicationDbContext>(builder =>
                builder.UseSqlServer(Configuration.GetConnectionString("TeknikAuthEntities"), sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => 
            {
                options.Password = new PasswordOptions()
                {
                    RequireDigit = false,
                    RequiredLength = 4,
                    RequiredUniqueChars = 1,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.ErrorUrl = "/Error/IdentityError";
                options.UserInteraction.ErrorIdParameter = "errorId";
            })
                .AddOperationalStore(options =>
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(Configuration.GetConnectionString("TeknikAuthEntities"), sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddConfigurationStore(options =>
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(Configuration.GetConnectionString("TeknikAuthEntities"), sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddAspNetIdentity<ApplicationUser>()
                .AddRedirectUriValidator<TeknikRedirectUriValidator>()
                .AddDeveloperSigningCredential();

            services.AddAuthorization(options =>
            {
                foreach (var policy in Policies.Get())
                {
                    options.AddPolicy(policy.Name, p =>
                    {
                        foreach (var scope in policy.Scopes)
                        {
                            p.RequireScope(scope);
                        }
                    });
                }
            });

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = config.UserConfig.IdentityServerConfig.Authority;
                    options.RequireHttpsMetadata = true;

                    options.ApiName = "auth-api";
                });

            services.AddTransient<IPasswordHasher<ApplicationUser>, TeknikPasswordHasher>();
            services.AddTransient<IProfileService, TeknikProfileService>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, Config config)
        {
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
                    Name = "TeknikAuthSession",
                    SecurePolicy = CookieSecurePolicy.Always,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                }
            });

            // Use Exception Handling
            app.UseErrorHandler(config);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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

            InitializeDbTestDataAsync(app, config).Wait();

            app.UseIdentityServer();
            
            app.UseMvcWithDefaultRoute();
        }

        private static async System.Threading.Tasks.Task InitializeDbTestDataAsync(IApplicationBuilder app, Config config)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                if (!context.Clients.Any())
                {
                    foreach (var client in Clients.Get(config))
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Resources.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Resources.GetApiResources(config))
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
