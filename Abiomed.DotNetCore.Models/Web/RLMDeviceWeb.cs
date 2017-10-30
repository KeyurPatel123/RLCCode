using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class RLMDeviceWeb
    {
        public string Rlmsn { get; set; }
        public string Rlmpw { get; set; }
        public string InstitutionSAPId { get; set; }
        public string Aicserialnumber { get; set; }
        public string Aicsoftwarenumber { get; set; }
    }
}
