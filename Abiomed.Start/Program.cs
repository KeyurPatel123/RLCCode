using Abiomed.DotNetCore.DependencyInjection;
using Abiomed.DotNetCore.RLR.Communications;
using Autofac;
using System;

namespace Abiomed.Start
{
    class Program
    {
        private static AutofacContainer autofac;

        static void Main(string[] args)
        {
            try
            {
                autofac = new AutofacContainer();
                autofac.Build();

                // todo fill out!
                InsecureTCPServer _tcpServer = AutofacContainer.Container.Resolve<InsecureTCPServer>();
                _tcpServer.Run();
            }
            catch (Exception e)
            {

            }
        }
    }
}
