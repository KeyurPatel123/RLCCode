/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerAuthenticationUpdateIndication.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class BearerAuthenticationUpdateIndication : BaseMessage
    {
        #region Private        
        private Definitions.Status _status;
        private int _userRef;
        private BearerAuthInformation _bearerAuthInformation;
        #endregion

        #region Public
        public Definitions.Status Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int UserRef
        {
            get { return _userRef; }
            set { _userRef = value; }
        }

        public BearerAuthInformation BearerAuthInformation
        {
            get { return _bearerAuthInformation; }
            set { _bearerAuthInformation = value; }
        }
        #endregion



    }
}
