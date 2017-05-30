﻿/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerChangeResponse.cs: 
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.Models
{
    public class BearerChangeResponse : BaseMessage
    {
        #region Private                
        private Definitions.Status _status = Definitions.Status.Unknown;        
        #endregion

        #region Public
        public Definitions.Status Status
        {
            get { return _status; }
            set { _status = value; }
        }
        #endregion

    }
}
