/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * FilterConfig.cs: Filter Config
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System.Web.Mvc;

namespace Abiomed.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
