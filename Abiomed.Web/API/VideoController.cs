/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * VideoController.cs: Video Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
using RestSharp;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace Abiomed.Web
{
    public class VideoController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        public object Get()
        {
            var data = new JavaScriptSerializer().Deserialize<object>(WowzaInfo());

            RLMStreamList rLMStreams = new RLMStreamList();
            // fa-question-circle-o add as icon default!?
            return data;
        }

        [HttpPost]
        public string Post([FromUri] string serialNumber)
        {
            var status = StartVideo(serialNumber);
            return status;
        }

        private string WowzaInfo()
        {
            var client = new RestClient("http://10.11.0.16:8087/v2/servers/_defaultServer_/vhosts/_defaultVHost_/applications/live/instances/_definst_");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            return content;
        }

        private string StartVideo(string serialNumber)
        {
            var client = new RestClient("http://localhost/RLR/api/Devices?serialNumber=" + serialNumber);
            var request = new RestRequest(Method.POST);
            IRestResponse response = client.Execute(request);           
            return response.Content;
        }
    }
}