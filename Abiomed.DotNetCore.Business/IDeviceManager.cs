using Abiomed.DotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abiomed.DotNetCore.Business
{
    public interface IDeviceManager
    {
        bool DeviceExists(string rlmSerial);

        bool CreateDevice(RLMDeviceWeb device);

    }
}
