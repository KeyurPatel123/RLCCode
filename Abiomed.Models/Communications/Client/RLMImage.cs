using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
