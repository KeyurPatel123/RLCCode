using System;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore.Models
{
    public class HospitalConfiguration : TableEntity
    {
        public string HospitalName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string HospitalLongitude { get; set; }
        public string HospitalLatitude { get; set; }
        public string HospitalType { get; set; }
        public string PhoneNumber { get; set; }
    }

    [Serializable]
    public class Hospital 
    {
        public string Address { get; set; }
        public string City { get; set; }
        [JsonProperty("county_name")]
        public string County { get; set; }
        [JsonProperty("hospital_name")]
        public string HospitalName { get; set; }
        [JsonProperty("location")]
        public Location HospitalPointCoordinates { get; set; }
        public string LocationState { get; set; }
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
        [JsonProperty("provider_id")]
        public string ProviderId { get; set; }
        public string State { get; set; }
        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }
    }

    [Serializable]
    public class Location
    {
        public string Type { get; set; }
        public List<string> Coordinates { get; set; }
    }
}
