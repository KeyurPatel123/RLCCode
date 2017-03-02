/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DeviceStatusController.cs: Device Status Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models.Communications;
using System.Collections.Generic;
using System.Web.Http;

namespace Abiomed.Web.API
{
    public class DeviceStatusController : ApiController
    {
        private static List<DeviceStatus> deviceStatusList = null;
        //ConnectionMultiplexer _redis;
        //IDatabase _db;
        //ISubscriber _sub;

        public DeviceStatusController()
        {
          //  _redis = ConnectionMultiplexer.Connect("localhost");
          //  _db = _redis.GetDatabase();
          //  _sub = _redis.GetSubscriber();
          //
          //  _sub.Subscribe(@"RLMUpdate", (channel, message) => {
          //      //deviceStatusList = (string)message;
          //  });
        }

        
        [HttpPost]
        public void Post([FromBody]List<DeviceStatus> deviceStatus)
        {            
            deviceStatusList = deviceStatus;
        }

        [HttpGet]
        public List<DeviceStatus> Get()
        {
            return deviceStatusList;
        }
    }
}
