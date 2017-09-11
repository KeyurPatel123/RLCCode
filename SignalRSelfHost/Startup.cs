using Abiomed.Business;
using Abiomed.Models;
using Abiomed.Repository;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;

namespace Abiomed.SignalRSelfHost
{
    public class Startup
    {
        //private Configuration _configuration;
        public Startup()
        {
            //_configuration = configuration;
        }

        public void Start()
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            using (WebApp.Start<Startup>("http://*:8080"))
            {
                Console.WriteLine("Server running on 8088");
                Console.ReadLine();
            }
        }

        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }

    }
}
