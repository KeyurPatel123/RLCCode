using Abiomed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    public interface IRLMCommunication
    {
        event EventHandler SendMessage;

        byte[] ProcessMessage(string deviceId, byte[] dataMessage,out RLMStatus status);

    }
}
