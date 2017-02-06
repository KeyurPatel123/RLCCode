using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
