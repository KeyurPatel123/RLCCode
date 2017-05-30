using System;
using Abiomed.DependencyInjection;
using Abiomed.RLR.Communications;
using Autofac;
using System.Diagnostics;
using Abiomed.Models;

namespace Abiomed.ConsoleCore
{
    class Program
    {
        private static AutofacContainer autofac;

        static void Main(string[] args)
        {
            try
            {
               // Trace.TraceInformation(@"Remote Link Server - Started");
                autofac = new AutofacContainer();
                autofac.Build();
                Configuration _configuration = AutofacContainer.Container.Resolve<Configuration>();

                if (_configuration.Security)
                {
                    ITCPServer _tcpServer = AutofacContainer.Container.Resolve<ITCPServer>();
                    _tcpServer.Run();
                }
                else
                {
                    InsecureTcpServer _tcpServer = AutofacContainer.Container.Resolve<InsecureTcpServer>();
                    _tcpServer.Run();
                }
            }
            catch (Exception e)
            {
                System.Console.Write(e.InnerException.ToString());
            }
        }
    }
}