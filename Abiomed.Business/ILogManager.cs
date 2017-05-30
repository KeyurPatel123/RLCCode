using Abiomed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    public interface ILogManager
    {
        Task Create<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType);
    }
}
