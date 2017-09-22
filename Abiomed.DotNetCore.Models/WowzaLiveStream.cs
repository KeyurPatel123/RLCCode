using System.Collections.Generic;

namespace Abiomed.DotNetCore.Models
{
    public class WowzaLiveStream
    {
        public string ServerName { get; set; }
        public List<IncomingStream> IncomingStreams { get; set; }
        public List<OutgoingStream> OutgoingStreams { get; set; }
    }

    public class IncomingStream
    {
        public string ApplicationInstance { get; set; }
        public string Name { get; set; }
        public string SourceIp { get; set; }
        public string IsRecordingSet { get; set; }
        public string IsStreamManagerStream { get; set; }
        public string IsPublishedToVOD { get; set; }
        public string IsConnected { get; set; }
        public string IsPtzEnabled { get; set; }
        public string PtzPollingInterval { get; set; }
    }

    public class OutgoingStream
    {
        public string Name { get; set; }
        public string SomeProp { get; set; }
        public string SomeProp2 { get; set; }
    }
}
