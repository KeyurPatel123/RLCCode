using Microsoft.Azure.Documents;
using System;
using static Abiomed.DotNetCore.Models.Definitions;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class Log<T> : Resource
    {
        private T _message;
        private LogMessageType _logMessageType;
        private LogType _logSeverityType;
        private string _deviceIpAddress;
        private string _rlmSerial;
        private string _collectionName;
        
        public T Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public LogMessageType LogMessageType
        {
            get { return _logMessageType; }
            set { _logMessageType = value; }
        }

        public LogType LogSeverityType
        {
            get { return _logSeverityType; }
            set { _logSeverityType = value; }
        }

        public string DeviceIpAddress
        {
            get { return _deviceIpAddress; }
            set { _deviceIpAddress = value; }
        }

        public string RLMSerial
        {
            get { return _rlmSerial; }
            set { _rlmSerial = value; }
        }

        public string CollectionName
        {
            get { return _collectionName; }
            set { _collectionName = value; }
        }
    }

    [Serializable]    
    public class AzureLog<T>  
    {
        private string _logMessageType;
        private string _logSeverityType;
        private string _deviceIpAddress;
        private string _rlmSerial;
        private T _message;
        
        public string LogMessageType
        {
            get { return _logMessageType; }
            set { _logMessageType = value; }
        }
       
        public string LogSeverityType
        {
            get { return _logSeverityType; }
            set { _logSeverityType = value; }
        }

        public string DeviceIpAddress
        {
            get { return _deviceIpAddress; }
            set { _deviceIpAddress = value; }
        }

        public string RLMSerial
        {
            get { return _rlmSerial; }
            set { _rlmSerial = value; }
        }
        
        public T Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
