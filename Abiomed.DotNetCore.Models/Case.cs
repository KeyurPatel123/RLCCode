using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class Case
    {
        public string RemoteLinkSerialNumber { get; set; } = string.Empty;
        public string PumpSerialNumber { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public string AicSerialNumber { get; set; } = string.Empty;
        public string AicSoftwareVersion { get; set; } = string.Empty;
        public string PerformanceLevel { get; set; } = string.Empty;
        public List<Tuple<DateTime, string>> PerformanceLevelHistory { get; set; }
        public Alarm Alarm1 { get; set; }
        public Alarm Alarm2 { get; set; }
        public Alarm Alarm3 { get; set; }
        public List<Tuple<DateTime, Alarm, Alarm, Alarm>> AlarmHistory { get; set; }
        public ImpellaFlow ImpellaFlow { get; set; } = new ImpellaFlow();
        public List<Tuple<DateTime, ImpellaFlow>> ImpellaFlowHistory {get;set;}
        public DateTime ConnectionStartUtc { get; set; } = DateTime.MinValue;
        public DateTime LastActiveUtc { get; set; } = DateTime.MinValue;
        public DateTime LastUpdateUtc { get; set; } = DateTime.MinValue;
        public bool Updated { get; set; } = false;
    }

    [Serializable]
    public class Alarm
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [Serializable]
    public class ImpellaFlow
    {
        public string Min = string.Empty;
        public string Max = string.Empty;
        public string Avg = string.Empty;
    }

    [Serializable]
    public class RemoteLinkCases
    {
        public ConcurrentDictionary<string, Case> Cases = new ConcurrentDictionary<string, Case>();
    }
}