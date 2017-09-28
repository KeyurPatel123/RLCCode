using Newtonsoft.Json;
using System;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class OcrResponse
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string BatchStartTimeUtc { get; set; } = string.Empty;
        public string ProcessDateTimeUtc { get; set; } = string.Empty;
        public string ImpellaSerialNumber { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string AicSerialNumber { get; set; } = string.Empty;
        public string SoftwareVersion { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public string IsDemo { get; set; } = string.Empty;
        public string PlacementSignal { get; set; } = string.Empty;
        public string PlacementSignalAverage { get; set; } = string.Empty;
        public string PLevel { get; set; } = string.Empty;
        public string MotorCurrent { get; set; } = string.Empty;
        public string MotorCurrentAverage { get; set; } = string.Empty;
        public string ImpellaFlow { get; set; } = string.Empty;
        public string ImpellaFlowMax { get; set; } = string.Empty;
        public string ImpellaFlowMin { get; set; } = string.Empty;
        public string PurgeFlow { get; set; } = string.Empty;
        public string PurgePressure { get; set; } = string.Empty;
        public string SystemPower { get; set; } = string.Empty;
        public string RawMessage { get; set; } = string.Empty;
        public string ResultStatusNote { get; set; } = string.Empty;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}