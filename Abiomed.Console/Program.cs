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
           // MakeRequest();            
            try
            {
                Trace.TraceInformation(@"Remote Link Server - Started");
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

        static async void MakeRequest()
        {
            var client = new RestClient("https://eastus2.api.cognitive.microsoft.com/vision/v1.0/ocr");
            var request = new RestRequest(Method.POST);
            request.AddQueryParameter("language", "en");
            request.AddHeader("Ocp-Apim-Subscription-Key", "ed94b5293b3347d29764f97c93bf5868");
            request.AddFile("image", @"C:\RLMImages\RL12345.png");
            request.AddHeader("Content -Type", "multipart/form-data");
            var response = client.Execute(request);


        }
    }
}
