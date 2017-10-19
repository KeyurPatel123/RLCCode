using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Storage;
using System.IO;
using Abiomed.WirelessRemoteLink;
using Abiomed.DotNetCore.Repository;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            services.AddIdentity<RemoteLinkUser, ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>((options) =>
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
                .AddDefaultTokenProviders()
                .CreateAzureTablesIfNotExists<ApplicationDbContext>();

            services.AddMvc();            

            services.AddAuthentication()
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;

                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Tokens:Issuer"],
                    ValidAudience = Configuration["Tokens:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                };
            });

            TableStorage tableStorage = new TableStorage();
            services.AddSingleton<ITableStorage>(tableStorage);
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            services.AddSingleton<IConfigurationManager>(configurationManager);
            ConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            configurationCache.LoadCacheAsync().Wait();
            services.AddSingleton<IConfigurationCache>(configurationCache);

            string auditLogTableName = !string.IsNullOrEmpty(tablePrefix) ? (tablePrefix + auditTableName) : auditTableName;
            AuditLogManager auditLogManager = new AuditLogManager(tableStorage, configurationCache);
            services.AddSingleton<IAuditLogManager>(auditLogManager);

            configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.Queue.ToString());
            configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Broadcaster.ToString());
            services.AddSingleton<IEmailManager>(new EmailManager(auditLogManager, configurationCache));

            RedisDbRepository<OcrResponse> redisDbRepository = new RedisDbRepository<OcrResponse>(configurationCache);
            services.AddSingleton<IDeviceManager>(new DeviceManager(configurationCache, redisDbRepository));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                                IHostingEnvironment env,
                                ILoggerFactory loggerFactory,
                                IAuditLogManager auditLogManager,
                                IEmailManager emailManager,
                                ITableStorage tableStorage,
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
            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "Home", action = "Index" });               
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
