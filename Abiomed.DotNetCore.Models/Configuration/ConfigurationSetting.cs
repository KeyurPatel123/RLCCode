using Newtonsoft.Json;

namespace Abiomed.DotNetCore.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ConfigurationSetting
    {
        [JsonProperty]
        public string Category { get; set; } = string.Empty;
        [JsonProperty]
        public string Name { get; set; } = string.Empty;
        [JsonProperty]
        public string Value { get; set; } = string.Empty;
    }
}
