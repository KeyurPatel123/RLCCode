/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DeviceStatus.cs: Device Status Manager
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Abiomed.Models;
using System;
using System.Collections.Generic;

namespace Abiomed.Web
{
    public class DeviceStatusManager : IDeviceStatusManager
    {
        private List<DeviceStatus> _devices = new List<DeviceStatus>();
        
        public List<DeviceStatus> Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }

        public void InitDevice(RLMDeviceList devices)
        {
            _devices = new List<DeviceStatus>();
            _devices.AddRange(Convert(devices));
        }

        public void AddDevice(RLMDevice device)
        {
            _devices.Add(Convert(device));
        }

        public void AddDevice(RLMDeviceList devices)
        {
            _devices.AddRange(Convert(devices));
        }

        public void DeleteDevice(string serialNumber)
        {            
            var index = _devices.FindIndex(x => x.SerialNumber == serialNumber);
            _devices.RemoveAt(index);
        }

        public void UpdateDevice(RLMDevice device)
        {
            var index = _devices.FindIndex(x => x.SerialNumber == device.SerialNo);
            _devices[index] = Convert(device);            
        }

        public void UpdateDevice(RLMDeviceList devices)
        {
            throw new NotImplementedException();
        }

        public List<DeviceStatus> Convert(RLMDeviceList devices)
        {
            List<DeviceStatus> deviceStatusList = new List<DeviceStatus>();
            foreach (var device in devices.RLMDevices)
            {
                DeviceStatus ds = new DeviceStatus { Bearer = device.Value.Bearer.ToString(), ConnectionTime = device.Value.ConnectionTime, SerialNumber = device.Value.SerialNo, DeviceIpAddress = device.Value.DeviceIpAddress };
                deviceStatusList.Add(ds);
            }

            return deviceStatusList;
        }

        public DeviceStatus Convert(RLMDevice device)
        {
            DeviceStatus ds = new DeviceStatus { Bearer = device.Bearer.ToString(), ConnectionTime = device.ConnectionTime, SerialNumber = device.SerialNo, DeviceIpAddress = device.DeviceIpAddress };
            return ds;
        }
    }
}
