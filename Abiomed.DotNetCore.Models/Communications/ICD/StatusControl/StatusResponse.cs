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
    [Serializable]
    public class StatusResponse : BaseMessage
    {
        #region Private        
        private BearerInformation _ethernet;
        private BearerInformation _wifi24;
        private BearerInformation _wifi5;
        private BearerInformation _lte;
        #endregion

        #region Public
        public BearerInformation Ethernet
        {
            get { return _ethernet; }
            set { _ethernet = value; }
        }

        public BearerInformation Wifi24
        {
            get { return _wifi24; }
            set { _wifi24 = value; }
        }

        public BearerInformation Wifi5
        {
            get { return _wifi5; }
            set { _wifi5 = value; }
        }

        public BearerInformation LTE
        {
            get { return _lte; }
            set { _lte = value; }
        }
        #endregion

    }
}
