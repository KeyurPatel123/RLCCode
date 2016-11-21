using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abiomed.DependencyInjection;
using Abiomed.CSR.Communications;
using Autofac;
// Testing   
namespace Abiomed.Console
{
    class Program
    {
        private static AutofacContainer autofac;
        static int Main(string[] args)
        {
            try
            {

                autofac = new AutofacContainer();
                autofac.Build();
                ITLSServer _tlsServer = AutofacContainer.Container.Resolve<ITLSServer>();

                _tlsServer.Run();                             
            }
            catch (Exception e)
            {
                System.Console.Write(e.InnerException.ToString());
            }
            return 0;     
        }
    }
}
