/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * IDeviceStatusManager.cs:
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using Abiomed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Web
{
    public interface IDeviceStatusManager
    {
        List<DeviceStatus> Devices { get; set; }

        DeviceStatus Convert(RLMDevice device);

        List<DeviceStatus> Convert(RLMDeviceList devices);

        void InitDevice(RLMDeviceList devices);


        void AddDevice(RLMDeviceList devices);    
        void AddDevice(RLMDevice device);

        void UpdateDevice(RLMDeviceList devices);
        void UpdateDevice(RLMDevice device);

        void DeleteDevice(string device);
       
    }
}
