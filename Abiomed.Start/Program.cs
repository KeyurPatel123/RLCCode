using Abiomed.DotNetCore.RLR.Communications;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abiomed.DotNetCore.Business;

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
                .BuildServiceProvider();

                //configure console logging
                serviceProvider
                    .GetService<ILoggerFactory>()                    
                    .AddConsole(LogLevel.Debug);

                var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
                logger.LogDebug("Starting application");

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
