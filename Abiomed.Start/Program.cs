using Abiomed.DotNetCore.RLR.Communications;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Storage;
using Microsoft.Extensions.Configuration;
using System.IO;
using Abiomed.RLR.DotNetCore.Communications;

namespace Abiomed.Start
{
    class Program
    {
        private static IConfigurationRoot _configuration { get; set; }

        static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");

                _configuration = builder.Build();

                string storageConnection = _configuration.GetSection("AzureAbiomedCloud:StorageConnection").Value;

                //setup our DI
                var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<InsecureTCPServer>()
                .AddSingleton<TCPServer>()
                .AddSingleton<RLMCommunication>()
                .AddSingleton<RLMDeviceList>()
                .AddSingleton<IDigitiserCommunication, DigitiserCommunication>()
                .AddSingleton<IFileTransferCommunication, FileTransferCommunication>()
                .AddSingleton<ISessionCommunication, SessionCommunication>()
                .AddSingleton<IStatusControlCommunication, StatusControlCommunication>()
                .AddSingleton<IRLMCommunication, RLMCommunication>()
                .AddSingleton<IKeepAliveManager, KeepAliveManager>()
                .AddSingleton<IConfigurationCache, ConfigurationCache>()
                .AddSingleton<IConfigurationManager, ConfigurationManager>()
                .AddSingleton<ITableStorage, TableStorage>()
                .AddScoped(typeof(IRedisDbRepository<>), typeof(RedisDbRepository<>))
                .BuildServiceProvider();

                //configure console logging
                serviceProvider
                    .GetService<ILoggerFactory>()                    
                    .AddConsole(LogLevel.Debug);

                var _logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
                                
                _logger.LogInformation("Starting RLR");

                var configurationCache = serviceProvider.GetService<IConfigurationCache>();
                configurationCache.LoadCache().Wait();

                var security = configurationCache.GetBooleanConfigurationItem("connectionmanager", "security");

                if(security)
                {
                    var _tcpServer = serviceProvider.GetService<TCPServer>();
                    _tcpServer.Run();
                }
                else
                {
                    var _tcpServer = serviceProvider.GetService<InsecureTCPServer>();
                    _tcpServer.Run();
                }
                
            }
            catch (Exception e)
            {
                Console.Write(e.InnerException);
            }
        }
    }
}
