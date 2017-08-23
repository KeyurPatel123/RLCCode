using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Abiomed.Models.Definitions;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace Abiomed.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Log<T> : Resource
    {
        private T _message;
        private LogMessageType _logMessageType;
        private LogType _logSeverityType;
        private string _deviceIpAddress;
        private string _rlmSerial;
        private string _collectionName;
        
        [JsonProperty]
        public T Message
        {
            get { return _message; }
            set { _message = value; }
        }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public LogMessageType LogMessageType
        {
            get { return _logMessageType; }
            set { _logMessageType = value; }
        }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public LogType LogSeverityType
        {
            get { return _logSeverityType; }
            set { _logSeverityType = value; }
        }

        [JsonProperty]
        public string DeviceIpAddress
        {
            get { return _deviceIpAddress; }
            set { _deviceIpAddress = value; }
        }

        [JsonProperty]
        public string RLMSerial
        {
            get { return _rlmSerial; }
            set { _rlmSerial = value; }
        }
        [JsonProperty]
        public string CollectionName
        {
            get { return _collectionName; }
            set { _collectionName = value; }
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class AzureLog<T>  
    {
        private string _logMessageType;
        private string _logSeverityType;
        private string _deviceIpAddress;
        private string _rlmSerial;
        private T _message;

        [JsonProperty]
        public string LogMessageType
        {
            get { return _logMessageType; }
            set { _logMessageType = value; }
        }

        [JsonProperty]
        public string LogSeverityType
        {
            get { return _logSeverityType; }
            set { _logSeverityType = value; }
        }

        [JsonProperty]
        public string DeviceIpAddress
        {
            get { return _deviceIpAddress; }
            set { _deviceIpAddress = value; }
        }
        [JsonProperty]
        public string RLMSerial
        {
            get { return _rlmSerial; }
            set { _rlmSerial = value; }
        }
        [JsonProperty]
        public T Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
