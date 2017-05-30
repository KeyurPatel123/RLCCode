/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * ClearFileResponse.cs: Client File Open Response Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    public class ClearFileResponse : BaseMessage
    {
        #region Private
        private int _status;
        private int _userRef;
        #endregion

        #region Public
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int UserRef
        {
            get { return _userRef; }
            set { _userRef = value; }
        }
        #endregion

    }
}
