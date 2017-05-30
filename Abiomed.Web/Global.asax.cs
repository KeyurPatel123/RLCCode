/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * Global.asax.cs: Start File of IIS Web Application
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Abiomed.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            AutofacContainer autofac = new AutofacContainer();
        }
    }
}
