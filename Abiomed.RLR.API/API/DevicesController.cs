/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DevicesController.cs: Devices Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
using Abiomed.Models.Communications;
using System.Collections.Generic;
using System.Web.Http;

namespace Abiomed.RLR.API.API
{
    public class DevicesController : ApiController
    {
        private RLMDeviceList _RLMDeviceList;

        public DevicesController(RLMDeviceList RLMDeviceList)
        {
            _RLMDeviceList = RLMDeviceList;
        }

        // POST api/<controller>
        [HttpPost]
        public bool Post([FromUri]string serialNumber)
        {
            // Send message to start streaming
            //var status = _rLMCommunication.StartVideo(serialNumber);
            return true;// status;
        }       

        [HttpGet]
        public List<DeviceStatus> Get()
        {
            List<DeviceStatus> devices = new List<DeviceStatus>();
            foreach (var device in _RLMDeviceList.RLMDevices)
            {
                DeviceStatus ds = new DeviceStatus { Bearer = device.Value.Bearer.ToString(), ConnectionTime = device.Value.ConnectionTime, SerialNumber = device.Value.SerialNo };
                devices.Add(ds);
            }

            return devices;
        }
    }
}
