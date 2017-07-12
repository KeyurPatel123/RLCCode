/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * CloseSessionRequest.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.Models
{
    public class SessionCloseRequest : BaseMessage
    {
        #region Private        
        private Definitions.Status _status = Definitions.Status.Unknown;
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        #endregion

        #region Public
        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }

        public Definitions.Status Status
        {
            get { return _status; }
            set { _status = value; }
        }
        #endregion
    }
}
