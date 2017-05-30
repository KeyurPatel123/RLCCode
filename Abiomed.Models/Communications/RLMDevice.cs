/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMDevice.cs: RLM Device Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System;
using System.Collections.Generic;
using static Abiomed.Models.Definitions;

namespace Abiomed.Models
{
    [Serializable]
    public class RLMDevice
    {
        #region Private        
        private string _deviceIpAddress = string.Empty;
        private DateTime _connectionTime = DateTime.MaxValue;        
        private string _serialNo = string.Empty;
        private int _clientSequence = 0;
        private UInt16 _serverSequence = 0;
        private int _ifaceVer = 0;
        private Definitions.Bearer _bearer = Definitions.Bearer.Unknown;
        private string _text = string.Empty;
        private List<byte> _dataTransfer = new List<byte>();        
        private int _currentBlock = 0;
        private int _totalBlocks = 0;
        private uint _fileTransferSize;
        private RLMFileTransfer _fileTransferType;
        private UInt16 _bearerSlotNumber = 0;
        private List<BearerAuthInformation> _bearerAuthInformationList;

        #endregion

        #region Public
        public string DeviceIpAddress
        {
            get { return _deviceIpAddress; }
            set { _deviceIpAddress = value; }
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

        public List<byte> DataTransfer
        {
            get { return _dataTransfer; }
            set { _dataTransfer = value; }
        }

        public int Block
        {
            get { return _currentBlock; }
            set { _currentBlock = value; }
        }

        public int TotalBlocks
        {
            get { return _totalBlocks; }
            set { _totalBlocks = value; }
        }

        public uint FileTransferSize
        {
            get { return _fileTransferSize; }
            set { _fileTransferSize = value; }
        }

        public RLMFileTransfer FileTransferType
        {
            get { return _fileTransferType; }
            set { _fileTransferType = value; }
        }

        public UInt16 BearerSlotNumber
        {
            get { return _bearerSlotNumber; }
            set { _bearerSlotNumber = value; }
        }

        public List<BearerAuthInformation> BearerAuthInformationList
        {
            get { return _bearerAuthInformationList; }
            set { _bearerAuthInformationList = value; }
        }

        #endregion
    }
}
