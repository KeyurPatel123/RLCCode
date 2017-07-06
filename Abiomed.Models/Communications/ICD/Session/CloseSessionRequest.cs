/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * CloseSessionRequest.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;

namespace Abiomed.Models
{
    [Serializable]
    public class CloseSessionRequest : BaseMessage
    {
        #region Private        
        private BearerStatistics _ethernet;
        private BearerStatistics _wifi24;
        private BearerStatistics _wifi5;
        private BearerStatistics _lte;
        
        #endregion

        #region Public
        public BearerStatistics Ethernet
        {
            get { return _ethernet; }
            set { _ethernet = value; }
        }

        public BearerStatistics Wifi24
        {
            get { return _wifi24; }
            set { _wifi24 = value; }
        }

        public BearerStatistics Wifi5
        {
            get { return _wifi5; }
            set { _wifi5 = value; }
        }

        public BearerStatistics LTE
        {
            get { return _lte; }
            set { _lte = value; }
        }
        #endregion
    }
}
