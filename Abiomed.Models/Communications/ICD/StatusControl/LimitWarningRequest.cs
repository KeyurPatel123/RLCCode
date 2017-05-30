/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * StatusResponse.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.Models
{
    public class LimitWarningRequest : BaseMessage
    {
        #region Private        
        private LimitRequest _limitRequest = new LimitRequest();
        #endregion

        #region Public
        public LimitRequest LimitRequest
        {
            get { return _limitRequest; }
            set { _limitRequest = value; }
        }       
        #endregion

    }
}
