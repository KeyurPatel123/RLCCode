/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMImage.cs: RLMImage Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    [Serializable]
    public class RLMImage
    {
        private byte[] _data;        
        private string _serialNumber;
        private DateTime _date = DateTime.UtcNow;
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
    }
}
