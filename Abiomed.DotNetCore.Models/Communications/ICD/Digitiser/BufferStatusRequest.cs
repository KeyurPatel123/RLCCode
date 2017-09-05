/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BufferStatusRequest.cs: Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class BufferStatusRequest : BaseMessage
    {
        private UInt16 _status = 0;
        private int _bytes = 0;
        private int _dropped = 0;
        private int _sent = 0;

        public UInt16 Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public int Bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        public int Dropped
        {
            get { return _dropped; }
            set { _dropped = value; }
        }

        public int Sent
        {
            get { return _sent; }
            set { _sent = value; }
        }
    }
}
