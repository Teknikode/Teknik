using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
using Teknik.WebCommon.Middleware;
using Microsoft.Extensions.Hosting;
using Teknik.Middleware;
using Teknik.WebCommon;
using Teknik.IdentityServer.Controllers;

namespace Teknik.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string dataDir = (Configuration != null) ? Configuration["ConfigDirectory"] : null;
            if (string.IsNullOrEmpty(dataDir))
            {
                string baseDir = Environment.ContentRootPath;
                dataDir = Path.Combine(baseDir, "App_Data");
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Create Configuration Singleton
            services.AddScoped<Config, Config>(opt => Config.Load(dataDir));

            // Build an intermediate service provider
            var sp = services.BuildServiceProvider();

            // Resolve the services from the service provider
            var config = sp.GetService<Config>();
            var devEnv = config?.DevEnvironment ?? true;
            var defaultConn = config?.DbConnection ?? string.Empty;
            var authority = config?.UserConfig?.IdentityServerConfig?.Authority ?? string.Empty;
            var signingCert = config?.UserConfig?.IdentityServerConfig?.SigningCertificate ?? string.Empty;

            if (devEnv)
            {
                Environment.EnvironmentName = Environments.Development;
            }
            else
            {
                Environment.EnvironmentName = Environments.Production;
            }

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "TeknikAuth";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (Environment.IsDevelopment()) ? StatusCodes.Status307TemporaryRedirect : StatusCodes.Status308PermanentRedirect;
#if DEBUG
                options.HttpsPort = 5050;
#else
                options.HttpsPort = 443;
#endif
            });

            services.AddScoped<IErrorController, ErrorController>();
            services.AddControllersWithViews()
                    .AddControllersAsServices()
                    .AddNewtonsoftJson();

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

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<ApplicationDbContext>(options => options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(defaultConn, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)), 
                    ServiceLifetime.Transient);

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

            var identityBuilder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.ErrorUrl = "/Error/IdentityError";
                options.UserInteraction.ErrorIdParameter = "errorId";
                options.Cors.CorsPaths.Add(new PathString("/connect/authorize"));
                options.Cors.CorsPaths.Add(new PathString("/connect/endsession"));
                options.Cors.CorsPaths.Add(new PathString("/connect/checksession"));
                options.Cors.CorsPaths.Add(new PathString("/connect/introspect"));
                options.Caching.ClientStoreExpiration = TimeSpan.FromHours(1);
            })
                .AddOperationalStore(options =>
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(defaultConn, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddConfigurationStore(options =>
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(defaultConn, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddConfigurationStoreCache()
                .AddAspNetIdentity<ApplicationUser>()
                .AddRedirectUriValidator<TeknikRedirectUriValidator>();

            if (!string.IsNullOrEmpty(signingCert))
            {
                identityBuilder.AddSigningCredential($"CN={signingCert}");
            }
            else
            {
                identityBuilder.AddDeveloperSigningCredential();
            }

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
                    options.Authority = authority;
                    options.RequireHttpsMetadata = true;

                    options.ApiName = "auth-api";
                });

            services.AddTransient<IPasswordHasher<ApplicationUser>, TeknikPasswordHasher>();
            services.AddTransient<IProfileService, TeknikProfileService>();
        }
        
        public void Configure(IApplicationBuilder app, ApplicationDbContext dbContext, Config config)
        {
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
                    Name = "TeknikAuthSession",
                    SecurePolicy = CookieSecurePolicy.Always,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                }
            });

            // Force a HTTPS redirection (301)
            app.UseHttpsRedirection();

            // Use Exception Handling
            app.UseErrorHandler();

            // Custom Middleware
            app.UseBlacklist();
            app.UseCORS();
            app.UseCSP();
            app.UseSecurityHeaders();

            // Cache Responses
            app.UseResponseCaching();

            if (config != null)
                InitializeDbTestDataAsync(app, config);

            app.UseIdentityServer();

            // Authorize all the things!
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private static void InitializeDbTestDataAsync(IApplicationBuilder app, Config config)
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

                if (!context.ApiScopes.Any())
                {
                    foreach (var apiScope in Resources.GetApiScopes())
                    {
                        context.ApiScopes.Add(apiScope.ToEntity());
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
