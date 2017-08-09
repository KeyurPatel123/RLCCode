/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * Program.cs: Console App. Start
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;
using System.Text;
using Abiomed.DependencyInjection;
using Abiomed.RLR.Communications;
using Autofac;
using System.Diagnostics;
using Abiomed.Models;
using Abiomed.FactoryData;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using Newtonsoft.Json;
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
                autofac = new AutofacContainer();
                autofac.Build();
                Configuration _configuration =  AutofacContainer.Container.Resolve<Configuration>();

                if (_configuration.Security)
                {
                    ITCPServer _tcpServer = AutofacContainer.Container.Resolve<ITCPServer>();
                    _tcpServer.Run();
                }
                else
                {
                    InsecureTcpServer _tcpServer = AutofacContainer.Container.Resolve<InsecureTcpServer>();
                    _tcpServer.Run();
                }                             
            }
            catch (Exception e)
            {
                System.Console.Write(e.InnerException.ToString());
            }
            return 0;     
        }
    }
}
