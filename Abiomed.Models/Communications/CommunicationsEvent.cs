using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
