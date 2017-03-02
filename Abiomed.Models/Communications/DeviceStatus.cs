/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * DeviceStatus.cs: Device Status Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models.Communications
{
    public class DeviceStatus
    {
        private DateTime _connectionTime;
        private string _serialNumber;
        private string _bearer;
        
        public DateTime ConnectionTime
        {
            get { return _connectionTime; }
            set { _connectionTime = value; }
        }
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }
        public string Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }        
    }
}
