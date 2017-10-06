using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Abiomed.DotNetCore.Models
{
    [Serializable]
    public class RLMDevices
    {
        public ConcurrentDictionary<string, OcrResponse> Devices = new ConcurrentDictionary<string, OcrResponse>();
    }
}
