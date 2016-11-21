using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class RLMDeviceList
    {
        #region Private        
        private Dictionary<string, RLMDevice> _RLMDeviceList = new Dictionary<string, RLMDevice>();
        #endregion

        #region Public
        public Dictionary<string, RLMDevice> RLMDevices
        {
            get { return _RLMDeviceList; }
            set { _RLMDeviceList = value; }
        }
        #endregion
    }
}
