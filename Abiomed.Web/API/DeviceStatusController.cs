using Abiomed.Models;
using Abiomed.Models.Communications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Abiomed.Web.API
{
    public class DeviceStatusController : ApiController
    {
        private static List<DeviceStatus> deviceStatusList = null;

        [HttpPost]
        public void Post([FromBody]List<DeviceStatus> deviceStatus)
        {
            //RLMDeviceList account = JsonConvert.DeserializeObject<RLMDeviceList>(rLMDeviceList.ToString());
            //
            //RLMDeviceList list = rLMDeviceList.ToObject<RLMDeviceList>();
            // todo set in BL
            deviceStatusList = deviceStatus;
        }

        [HttpGet]
        public List<DeviceStatus> Get()
        {
            return deviceStatusList;
        }
    }
}
