/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BaseMessage.cs: Base Message from Client or Server Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using Newtonsoft.Json;
using System;

namespace Abiomed.Models
{
    [Serializable]
    public class BaseMessage
    {
        #region Private
        private UInt16 _msgId;
        private UInt16 _msgLen;
        private UInt16 _msgSeq;        
        #endregion

        #region Public
        [JsonProperty]
        public UInt16 MsgId
        {
            get { return _msgId; }
            set { _msgId = value; }
        }

        [JsonProperty]
        public UInt16 MsgLen
        {
            get { return _msgLen; }
            set { _msgLen = value; }
        }

        public UInt16 MsgSeq
        {
            get { return _msgSeq; }
            set { _msgSeq = value; }
        }
        #endregion

    }
}
