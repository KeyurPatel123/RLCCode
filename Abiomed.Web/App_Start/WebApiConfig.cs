/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * WebApiConfig.cs: Web Api Config
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System.Web.Http;

public class WebApiConfig
{
    public static void Register(HttpConfiguration configuration)
    {
        configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{id}",
          new { id = RouteParameter.Optional });
    }
}