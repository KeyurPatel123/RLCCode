/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DeviceStatusController.cs: Device Status Controller
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models;
using Abiomed.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;

namespace Abiomed.Web.API
{
    public class DeviceStatusController : ApiController
    {
        private IDeviceStatusManager _deviceStatusManager;
        private IEventManager _eventManager;

        // Todo refactor out Redis and put in own manager
        public DeviceStatusController(IDeviceStatusManager deviceStatusManager, IEventManager eventManager)
        {
            _deviceStatusManager = deviceStatusManager;
            _eventManager = eventManager;
        }        
        

        [HttpGet]
        public List<DeviceStatus> Get()
        {
            return _deviceStatusManager.Devices;
        }
        
        [HttpPost]
        [Route("api/DeviceStatus/SendKeepAlive/{serialNumber}")]
        public void KeepAlive([FromUri]string serialNumber)
        {
           _eventManager.KeepAliveIndication(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/RLRLog/{serialNumber}")]
        public string RLRLog([FromUri]string serialNumber)
        {
            // Ask for new version
            _eventManager.OpenRLMLogFileIndication(serialNumber);

            // Get last known version
            // Search for all images with serial number
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(@"c:\\RLMLogs");
            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles(serialNumber + "*");

            string text = string.Empty;
            if (filesInDir.Length > 0)
            {
                // Get latest
                text = File.ReadAllText(filesInDir[filesInDir.Length -1].FullName);
            }

            return text;
        }

        [HttpPost]
        [Route("api/DeviceStatus/GetBearerInfo/{serialNumber}")]
        public void GetBearerInfo([FromUri]string serialNumber)
        {
            // Ask for updated list
            _eventManager.BearerAuthenticationReadIndication(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/SendVideoStart/{serialNumber}")]
        public void VideoStart([FromUri]string serialNumber)
        {
            _eventManager.StreamingVideoControlIndication(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/SendVideoStop/{serialNumber}")]
        public void VideoStop([FromUri]string serialNumber)
        {
            _eventManager.VideoStop(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/SendImageStart/{serialNumber}")]
        public void ImageStart([FromUri]string serialNumber)
        {
            _eventManager.ScreenCaptureIndication(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/SendImageStop/{serialNumber}")]
        public void ImageStop([FromUri]string serialNumber)
        {
            _eventManager.ImageStop(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/CloseSessionIndication/{serialNumber}")]
        public void CloseSessionIndication([FromUri]string serialNumber)
        {
            _eventManager.CloseSessionIndication(serialNumber);
        }

        [HttpPost]
        [Route("api/DeviceStatus/BearerChangeIndication/{serialNumber}/{bearer}")]
        public void SendUpdateBearer([FromUri]string serialNumber, string bearer)
        {
            _eventManager.BearerChangeIndication(serialNumber, bearer);
        }

        [HttpPost]
        [Route("api/DeviceStatus/CreateCredential")]
        public void CreateCredential([FromBody]Authorization Device)
        {
            _eventManager.BearerAuthenticationUpdateIndication(Device, false);
        }

        [HttpPost]
        [Route("api/DeviceStatus/DeleteCredential")]
        public void DeleteCredential([FromBody]Authorization Device)
        {
            _eventManager.BearerAuthenticationUpdateIndication(Device, true);
        }        
    }


}
