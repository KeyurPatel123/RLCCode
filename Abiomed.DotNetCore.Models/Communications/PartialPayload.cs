/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * TCPStateObject.cs: TCP State Object Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;

namespace Abiomed.DotNetCore.Models
{
    public class PartialPayload
    {
        private List<byte> _message = new List<byte>();
        private int _size = 0;
        
        public List<byte> Message
        {
            get { return _message; }
            set { _message = value; }
        }
        
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
    }
}
