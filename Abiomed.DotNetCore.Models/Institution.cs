using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class Institution
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SalesForceId { get; set; } = string.Empty;
        public string SapCustomerId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public GeographicCoordinate Coordinate { get; set; } = new GeographicCoordinate();
    }

    [Serializable]
    public class GeographicCoordinate
    {
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
    }
}
