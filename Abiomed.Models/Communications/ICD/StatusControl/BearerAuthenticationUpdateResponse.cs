/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerAuthenticationUpdateResponse.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    public class BearerAuthenticationUpdateResponse : BaseMessage
    {
        #region Private        
        private UInt16 _status;
        private int _userRef;
        private int _slot;        
        #endregion

        #region Public
        public UInt16 Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int UserRef
        {
            get { return _userRef; }
            set { _userRef = value; }
        }

        public int Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }
        #endregion

    }
}
