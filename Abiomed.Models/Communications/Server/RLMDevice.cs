/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMDevice.cs: RLM Device Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;

namespace Abiomed.Models
{
    public class RLMDevice
    {
        #region Private        
        private string _identifier;
        private DateTime _connectionTime;        
        private string _serialNo = string.Empty;
        private bool _streaming = false;
        private int _clientSequence = 0;
        private UInt16 _serverSequence = 0;
        private int _ifaceVer;
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        private string _text;
        private KeepAliveTimer _keepAliveTimer;
        private int _block = 0;
        private bool fileTransfer = false;

        #endregion

        #region Public
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }

        public DateTime ConnectionTime
        {
            get { return _connectionTime; }
            set { _connectionTime = value; }
        }
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

        public int Block
        {
            get { return _block; }
            set { _block = value; }
        }

        public bool FileTransfer
        {
            get { return fileTransfer; }
            set { fileTransfer = value; }
        }


        #endregion
    }
}
