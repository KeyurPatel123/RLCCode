/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * EventManager.cs: Event Handler
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models;
using Abiomed.Repository;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System;

namespace Abiomed.Web
{
    public class EventManager : IEventManager
    {
        private IRedisDbRepository<RLMDevice> _redisDbRepository;
        private IRedisDbRepository<RLMImage> _redisImage;

        private IDeviceStatusManager _deviceStatusManager;
        public EventManager(IRedisDbRepository<RLMDevice> redisDbRepository, IDeviceStatusManager deviceStatusManager, IRedisDbRepository<RLMImage> redisImage)
        {
            _redisDbRepository = redisDbRepository;
            _deviceStatusManager = deviceStatusManager;
            _redisImage = redisImage;
            
            // Register on new devices and Update Current List
            Register();
            GetCurrentState();
        }
       
        private void Register()
        {
            _redisDbRepository.Subscribe(Definitions.AddRLMDevice, (channel, message) =>
            {
                AddDevice(message);
            });

            _redisDbRepository.Subscribe(Definitions.UpdateRLMDevice, (channel, message) =>
            {
                UpdateDevice(message);
            });

            _redisDbRepository.Subscribe(Definitions.DeleteRLMDevice, (channel, message) =>
            {
                DeleteDevice(message);
            });

            _redisDbRepository.Subscribe(Definitions.ImageCapture, (channel, message) =>
            {
                SaveImage(message);
            });
        }

        private void GetCurrentState()
        {
            // Get Current Active List of RLM's
            string[] activeDevices = _redisDbRepository.GetSet(Definitions.RLMDeviceSet);

            RLMDeviceList rlmDeviceListLocal = new RLMDeviceList();

            // todo get all in one query!
            // Create List of devices
            foreach(var activeDevice in activeDevices)
            {
                RLMDevice device = _redisDbRepository.StringGet(activeDevice);
                rlmDeviceListLocal.RLMDevices.TryAdd(device.DeviceIpAddress, device);
            }

            // Set to global list
            _deviceStatusManager.InitDevice(rlmDeviceListLocal);
        }        

        private void AddDevice(string addedDevice)
        {
            RLMDevice device = _redisDbRepository.StringGet(addedDevice);
            _deviceStatusManager.AddDevice(device);
        }

        private void UpdateDevice(string updatedDevice)
        {
            RLMDevice device = _redisDbRepository.StringGet(updatedDevice);
            _deviceStatusManager.UpdateDevice(device);
        }

        private void DeleteDevice(string deletedDevice)
        {
            _deviceStatusManager.DeleteDevice(deletedDevice);
        }

        private void SaveImage(string serialNumber)
        {
            RLMImage rlmImage = _redisImage.StringGet(serialNumber);

            using (Image image = Image.FromStream(new MemoryStream(rlmImage.Data)))
            {
                // Create Name : RLXXXXX_UTCTime
                StringBuilder fileName = new StringBuilder();
                fileName.Append(@"C:\\RLMImages\");
                fileName.Append(serialNumber);
                fileName.Append("-");                
                fileName.Append(rlmImage.Date.ToString("yyyyMMdd_hhmmss"));
                fileName.Append(".png");

                string fileNameStr = fileName.ToString();
                                

                // For now save locally
                image.Save(fileNameStr, ImageFormat.Png);  // Or Png
            }
        }

        /*
         * TODO! Need to figure out a better way to put messages in queue! Event HUB!
         */
        public void KeepAliveIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.KeepAliveIndicationEvent, device.DeviceIpAddress);
        }

        public void BearerChangeIndication(string serialNumber, string bearer)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);

            StringBuilder build = new StringBuilder(device.DeviceIpAddress);
            build.Append("-");
            build.Append(bearer);
            _redisDbRepository.Publish(Definitions.BearerChangeIndicationEvent, build.ToString());
        }

        public void StatusIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.StatusIndicationEvent, device.DeviceIpAddress);
        }

        public void BearerAuthenticationReadIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.BearerAuthenticationReadIndicationEvent, device.DeviceIpAddress);
        }

        public void BearerAuthenticationUpdateIndication(Authorization authorization, bool delete)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == authorization.SerialNumber);

            if(delete)
            {
                StringBuilder build = new StringBuilder(device.DeviceIpAddress);
                build.Append("-");
                build.Append(authorization.AuthorizationInfo.Slot);

                _redisDbRepository.Publish(Definitions.BearerAuthenticationUpdateIndicationEvent, build.ToString());
            }
            else
            {
                // Build string - Serial#, slot, bearer, authentication type, SSID, PSK
                StringBuilder build = new StringBuilder(device.DeviceIpAddress);                
                build.Append("-");
                build.Append(authorization.AuthorizationInfo.Slot);
                build.Append("-");
                build.Append(authorization.AuthorizationInfo.BearerType);
                build.Append("-");
                build.Append(authorization.AuthorizationInfo.AuthType);
                build.Append("-");
                build.Append(authorization.AuthorizationInfo.SSID);
                build.Append("-");
                build.Append(authorization.AuthorizationInfo.PSK);
                _redisDbRepository.Publish(Definitions.BearerAuthenticationUpdateIndicationEvent, build.ToString());
            }
        }

        public void StreamingVideoControlIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.StreamingVideoControlIndicationEvent, device.DeviceIpAddress);
        }

        public void ScreenCaptureIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.ScreenCaptureIndicationEvent, device.DeviceIpAddress);
        }

        public void VideoStop(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.VideoStopEvent, device.DeviceIpAddress);
        }

        public void ImageStop(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.ImageStopEvent, device.DeviceIpAddress);
        }

        public void OpenRLMLogFileIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.OpenRLMLogFileIndicationEvent, device.DeviceIpAddress);
        }

        public void CloseSessionIndication(string serialNumber)
        {
            var device = _deviceStatusManager.Devices.Find(x => x.SerialNumber == serialNumber);
            _redisDbRepository.Publish(Definitions.CloseSessionIndicationEvent, device.DeviceIpAddress);
        }        
    }
}