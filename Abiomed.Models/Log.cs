using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Abiomed.Models.Definitions;

namespace Abiomed.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Log<T> : Resource
    {
        private T _message;
        private LogMessageType _logMessageType;
        private string _deviceIpAddress;
        private string _rlmSerial;
        
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
    }
}
