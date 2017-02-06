using Abiomed.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Abiomed.RLR.API.API
{
    public class DevicesController : ApiController
    {
        private IRLMCommunication _rLMCommunication;

        public DevicesController(IRLMCommunication rLMCommunication)
        {
            _rLMCommunication = rLMCommunication;
        }

        // POST api/<controller>
        [HttpPost]
        public bool Post([FromUri]string serialNumber)
        {
            // Send message to start streaming
            var status = _rLMCommunication.StartVideo(serialNumber);
            return status;
        }       
    }
}
