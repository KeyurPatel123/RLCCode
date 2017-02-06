using Abiomed.RLR.Communications;
using Autofac;
using log4net;
using System.Web.Http;

namespace Abiomed.RLR.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Register
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Autofac 
            AutofacContainer autofac = new AutofacContainer();

            // Start TLS Service
            //ITLSServer _tlsServer = AutofacContainer.Container.Resolve<ITLSServer>();            
            //_tlsServer.Run();   
        }

    }   
}
