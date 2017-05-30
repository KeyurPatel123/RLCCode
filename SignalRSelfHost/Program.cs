using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using Autofac;

namespace Abiomed.SignalRSelfHost
{
    class Program
    {
        private static AutofacContainer autofac;

        static void Main(string[] args)
        {
            autofac = new AutofacContainer();
            autofac.Build();

            Startup _startup = AutofacContainer.Container.Resolve<Startup>();
            _startup.Start();            
        }        
    }      
}
