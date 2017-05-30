/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerAuthenticationReadResponse.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.Models
{
    public class BearerAuthenticationReadResponse : BaseMessage
    {
        #region Private        
        private Definitions.Status _status;
        private int _userRef;
        private int _slot;        
        private int _bearer;
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

        public int Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }

        public int Bearer
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
