/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMImage.cs: RLMImage Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.Models.Communications
{
    public class RLMImage
    {
        private byte[] _data;        
        private string _serialNumber;

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

    }
}
