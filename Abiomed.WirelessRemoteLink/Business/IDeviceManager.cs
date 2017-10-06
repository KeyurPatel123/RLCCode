using Abiomed.DotNetCore.Models;

namespace Abiomed.WirelessRemoteLink
{
    public interface IDeviceManager
    {
        RLMDevices GetRlmDevices();
    }
}
