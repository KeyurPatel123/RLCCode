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
        public string SerialNumber { get; set; } = string.Empty;
        public string ScreenName { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public string PumpSerialNumber { get; set; } = string.Empty;
        public string AicSerialNumber { get; set; } = string.Empty;
        public string AicSoftwareVersion { get; set; } = string.Empty;
        public string IsDemo { get; set; } = string.Empty;
        public string PlacementSignalSystole { get; set; } = string.Empty;
        public string PlacementSignalDistole { get; set; } = string.Empty;
        public string PlacementSignalAverage { get; set; } = string.Empty;
        public string PerformanceLevel { get; set; } = string.Empty;
        public string MotorCurrentSystole { get; set; } = string.Empty;
        public string MotorCurrentDistole { get; set; } = string.Empty;
        public string MotorCurrentAverage { get; set; } = string.Empty;
        public string FlowRateAverage { get; set; } = string.Empty;
        public string FlowRateMax { get; set; } = string.Empty;
        public string FlowRateMin { get; set; } = string.Empty;
        public string PurgeFlow { get; set; } = string.Empty;
        public string PurgePressure { get; set; } = string.Empty;
        public string Battery { get; set; } = string.Empty;
        public string RawMessage { get; set; } = string.Empty;
        public string ResultStatusNote { get; set; } = string.Empty;
        public string Alarm1 { get; set; } = AlarmCodes.None.ToString();
        public string Alarm2 { get; set; } = AlarmCodes.None.ToString();
        public string Alarm3 { get; set; } = AlarmCodes.None.ToString();
        public string Alarm1Message { get; set; } = string.Empty;
        public string Alarm2Message { get; set; } = string.Empty;
        public string Alarm3Message { get; set; } = string.Empty;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public enum ScreenName
    {
        Unknown = -1,
        PlacementSignal = 0
    }

    public enum AlarmCodes
    {
        Blank = -1,
        White = 0,
        Yellow = 1,
        Red = 2
    }
}