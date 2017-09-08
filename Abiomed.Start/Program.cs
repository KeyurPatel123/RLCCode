using Abiomed.DotNetCore.RLR.Communications;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Repository;

namespace Abiomed.Start
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //setup our DI
                var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<InsecureTCPServer>()
                .AddSingleton<RLMCommunication>()
                .AddSingleton<RLMDeviceList>()
                .AddSingleton<IDigitiserCommunication, DigitiserCommunication>()
                .AddSingleton<IFileTransferCommunication, FileTransferCommunication>()
                .AddSingleton<ISessionCommunication, SessionCommunication>()
                .AddSingleton<IStatusControlCommunication, StatusControlCommunication>()
                .AddSingleton<IRLMCommunication, RLMCommunication>()
                .AddSingleton<IKeepAliveManager, KeepAliveManager>()
                .AddSingleton<DotNetCore.Models.Configuration>()
                .AddScoped(typeof(IRedisDbRepository<>), typeof(RedisDbRepository<>))
                .BuildServiceProvider();

                //configure console logging
                serviceProvider
                    .GetService<ILoggerFactory>()                    
                    .AddConsole(LogLevel.Debug);

                var _logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
                                
                _logger.LogInformation("Starting RLR");

                var _tcpServer = serviceProvider.GetService<InsecureTCPServer>();
                _tcpServer.Run();
            }
            catch (Exception e)
            {
                Console.Write(e.InnerException);
            }
        }
    }
}
