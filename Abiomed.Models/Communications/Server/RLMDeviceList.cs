﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class RLMDeviceList
    {
        #region Private        
        private ConcurrentDictionary<string, RLMDevice> _RLMDeviceList = new ConcurrentDictionary<string, RLMDevice>();
        #endregion

        #region Public
        public ConcurrentDictionary<string, RLMDevice> RLMDevices
        {
            get { return _RLMDeviceList; }
            set { _RLMDeviceList = value; }
        }
        #endregion
    }
}
