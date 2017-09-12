using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Abiomed.Storage;
using Abiomed.Models;
using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.Configuration;

namespace Abiomed_WirelessRemoteLink
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string storageConnection = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:StorageConnectionString").Value;
            string tablePrefix = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:TablePrefix").Value;
            string auditTableName = Configuration.GetSection("Auditing:TableName").Value;
            string emailQueueName = Configuration.GetSection("Email:ServiceQueue:QueueName").Value;
            string emailQueueConnectionString = Configuration.GetSection("Email:ServiceQueue:QueueStorageConnectionString").Value;

            // Add Elcamino Azure Table Identity services.
            services.AddIdentity<RemoteLinkUser, IdentityRole>((options) =>
            {
                options.User.RequireUniqueEmail = GetBooleanConfigurationItem("Authentication:UserRequireUniqueEmail");
                options.SignIn.RequireConfirmedEmail = GetBooleanConfigurationItem("Authentication:SignInRequireConfirmedEmail");
                options.SignIn.RequireConfirmedPhoneNumber = GetBooleanConfigurationItem("Authentication:SignInRequireConfirmedPhoneNumber");

                options.Password.RequireDigit = GetBooleanConfigurationItem("Authentication:PasswordRequireDigit");
                options.Password.RequiredLength = GetNumericConfigurationItem("Authentication:PasswordRequiredLength", 8);

                options.Password.RequireNonAlphanumeric = GetBooleanConfigurationItem("Authentication:PasswordRequireNonAlphanumeric");
                options.Password.RequireUppercase = GetBooleanConfigurationItem("Authentication:PasswordRequireLowerCase");
                options.Password.RequireLowercase = GetBooleanConfigurationItem("Authentication:PasswordRequireUpperCase");

                int lockoutMinutes = GetNumericConfigurationItem("Authentication:LockoutMinutes", 30);
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutMinutes);
                options.Lockout.MaxFailedAccessAttempts = GetNumericConfigurationItem("Authentication:LockoutMaxFailedAccessAttempts", 5);
                options.Lockout.AllowedForNewUsers = GetBooleanConfigurationItem("Authentication:LockoutAllowedForNewUsers");
            })
                .AddAzureTableStores<ApplicationDbContext>(new Func<IdentityConfiguration>(() =>
                {
                    IdentityConfiguration idconfig = new IdentityConfiguration();
                    idconfig.TablePrefix = tablePrefix;
                    idconfig.StorageConnectionString = storageConnection;
                    idconfig.LocationMode = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:LocationMode").Value;
                    return idconfig;
                }))
                .AddDefaultTokenProviders();
               // .CreateAzureTablesIfNotExists<ApplicationDbContext>(); 

            services.AddMvc();

            Abiomed.DotNetCore.Storage.TableStorage tableStorage = new Abiomed.DotNetCore.Storage.TableStorage();
            services.AddSingleton<Abiomed.DotNetCore.Storage.ITableStorage>(tableStorage);
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            services.AddSingleton<IConfigurationManager>(configurationManager);
            ConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            configurationCache.LoadCache().Wait();
            services.AddSingleton<IConfigurationCache>(configurationCache);

            string auditLogTableName = !string.IsNullOrEmpty(tablePrefix) ? (tablePrefix + auditTableName) : auditTableName;
            AuditLogManager auditLogManager = new AuditLogManager(tableStorage, configurationCache);
            services.AddSingleton<IAuditLogManager>(auditLogManager);

            configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.Queue.ToString());
            configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Broadcaster.ToString());
            services.AddSingleton<IEmailManager>(new EmailManager(auditLogManager, configurationCache));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                                IHostingEnvironment env, 
                                ILoggerFactory loggerFactory, 
                                IAuditLogManager auditLogManager, 
                                IEmailManager emailManager, 
                                Abiomed.DotNetCore.Storage.ITableStorage tableStorage,
                                IConfigurationManager configurationManager,
                                IConfigurationCache configurationCache)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        private bool GetBooleanConfigurationItem(string key)
        {
            bool.TryParse(Configuration.GetSection(key).Value, out bool result);
            return result;
        }

        private int GetNumericConfigurationItem(string key, int defaultValue)
        {
            if (!int.TryParse(Configuration.GetSection(key).Value, out int result))
            {
                result = defaultValue;
            }
            return result;
        }
    }
}
