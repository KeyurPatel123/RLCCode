/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerOpen.cs: Client Bearer Open Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    [Serializable]
    public class CloseBearerRequest : BaseMessage
    {
        #region Private        
        private BearerStatistics _bearerStatistics;        
        #endregion

        #region Public
        public BearerStatistics BearerStatistics
        {
            get { return _bearerStatistics; }
            set { _bearerStatistics = value; }
        }        
        #endregion
    }
}
