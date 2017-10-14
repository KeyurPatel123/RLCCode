﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Storage;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Repository;

namespace Abiomed.RLR
{
    class Program
    {
        private static IConfigurationRoot _configuration { get; set; }

        static void Main(string[] args)
        {
            try
            {
                // Get the current settings.
                int minWorker, minIOC;
                ThreadPool.GetMinThreads(out minWorker, out minIOC);
                ThreadPool.SetMinThreads(1000, 1000);

                var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");

                _configuration = builder.Build();

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
                .AddSingleton(typeof(IRedisDbRepository<>), typeof(RedisDbRepository<>))
                .BuildServiceProvider();

                //configure console logging
                var _logger = serviceProvider
                    .GetService<ILoggerFactory>()
                    .AddConsole(LogLevel.Trace)                    
                    .CreateLogger<Program>();  
                                                
                _logger.LogInformation("Starting RLR");

                var configurationCache = serviceProvider.GetService<IConfigurationCache>();
                configurationCache.LoadCacheAsync().Wait();

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
