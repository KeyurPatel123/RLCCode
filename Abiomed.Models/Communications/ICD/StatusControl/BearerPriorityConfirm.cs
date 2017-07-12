/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerPriorityConfirm.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    public class BearerPriorityConfirm : BaseMessage
    {
        #region Private        
        private UInt16 _status = UInt16.MaxValue;
        #endregion

        #region Public
        public UInt16 Status
        {
            get { return _status; }
            set { _status = value; }
        }        
        #endregion

    }
}
