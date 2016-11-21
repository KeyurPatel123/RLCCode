using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class RLMDevice
    {
        #region Private        
        private string _serialNo = string.Empty;
        private bool _streaming = false;
        private int _clientSequence = 0;
        private UInt16 _serverSequence = 0;
        private int _ifaceVer;
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        private string _text;
        private KeepAliveTimer _keepAliveTimer;        
        #endregion

        #region Public
        public string SerialNo
        {
            get { return _serialNo; }
            set { _serialNo = value; }
        }

        public bool Streaming
        {
            get { return _streaming; }
            set { _streaming = value; }
        }

        public int ClientSequence
        {
            get { return _clientSequence; }
            set { _clientSequence = value; }
        }

        public UInt16 ServerSequence
        {
            get { return _serverSequence; }
            set { _serverSequence = value; }
        }

        public int IfaceVer
        {
            get { return _ifaceVer; }
            set { _ifaceVer = value; }
        }

        public Definitions.Bearer Bearer
        {
            get { return _bearer; }
            set { _bearer = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public KeepAliveTimer KeepAliveTimer
        {
            get { return _keepAliveTimer; }
            set { _keepAliveTimer = value; }
        }
        #endregion
    }
}
