/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RouteConfig.cs: Route Config
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Web.Mvc;
using System.Web.Routing;

namespace Abiomed.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");  
            routes.MapRoute(
                name: "Default",
                url: "{*anything}",
                //url: "{controller}/{action}/{id}",
                defaults: new {controller = "RLM", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

