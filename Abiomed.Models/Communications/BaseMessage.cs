using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class BaseMessage
    {
        #region Private
        private UInt16 _msgId;
        private UInt16 _msgLen;
        private UInt16 _msgSeq;        
        #endregion

        #region Public
        public UInt16 MsgId
        {
            get { return _msgId; }
            set { _msgId = value; }
        }

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
