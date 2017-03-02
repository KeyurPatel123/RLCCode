/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMDeviceList.cs: RLM Device List Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Collections.Concurrent;

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
