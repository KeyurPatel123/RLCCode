/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerAuthenticationReadResponse.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class BearerAuthenticationReadResponse : BaseMessage
    {
        #region Private        
        private Definitions.Status _status = Definitions.Status.Unknown;
        private int _userRef = int.MaxValue;
        private int _slot = int.MaxValue;        
        private Definitions.Bearer _bearer = Definitions.Bearer.Wifi24Ghz;
        private BearerAuthInformation _bearerAuthInformation = new BearerAuthInformation();
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

        public int Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }

        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }

        public BearerAuthInformation BearerAuthInformation
        {
            get { return _bearerAuthInformation; }
            set { _bearerAuthInformation = value; }
        }
        #endregion

    }
}
