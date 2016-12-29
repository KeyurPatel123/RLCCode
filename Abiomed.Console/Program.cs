using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abiomed.DependencyInjection;
using Abiomed.RLR.Communications;
using Autofac;
// Testing
namespace Abiomed.Console
{
    public class Program
    {
        private static AutofacContainer autofac;
        static int Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("Enter port number ");
                autofac = new AutofacContainer();
                autofac.Build();
                ITCPServer _tcpServer = AutofacContainer.Container.Resolve<ITCPServer>();
                
                _tcpServer.Run();                             
            }
            catch (Exception e)
            {
                System.Console.Write(e.InnerException.ToString());
            }
            return 0;     
        }
    }
}
