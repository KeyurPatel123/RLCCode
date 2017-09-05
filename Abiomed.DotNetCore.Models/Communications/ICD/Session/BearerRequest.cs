/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerOpen.cs: Client Bearer Open Msg Struct Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class BearerRequest : BaseMessage
    {
        #region Private        
        private string _serialNo = "";
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;        
        #endregion

        #region Public
        public string SerialNo
        {
            get { return _serialNo; }
            set { _serialNo = value; }
        }

        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }
        #endregion

    }
}
