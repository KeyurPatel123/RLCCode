/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * CommunicationsEvent.cs: Communication Event Trigger
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    public class CommunicationsEvent : EventArgs
    {
        private string _identifier;
        private byte[] _message;
        
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }

        public byte[] Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
