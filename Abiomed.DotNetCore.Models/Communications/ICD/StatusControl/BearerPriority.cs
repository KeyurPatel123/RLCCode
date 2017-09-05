/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * StatusResponse.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.DotNetCore.Models
{
    public class BearerPriority : BaseMessage
    {
        #region Private        
        private UInt16 _ethernet = UInt16.MaxValue;
        private UInt16 _wifi = UInt16.MaxValue;
        private UInt16 _cellular = UInt16.MaxValue;
        #endregion

        #region Public
        public UInt16 Ethernet
        {
            get { return _ethernet; }
            set { _ethernet = value; }
        }

        public UInt16 WiFi
        {
            get { return _wifi; }
            set { _wifi = value; }
        }

        public UInt16 Cellular
        {
            get { return _cellular; }
            set { _cellular = value; }
        }    
        #endregion

    }
}
