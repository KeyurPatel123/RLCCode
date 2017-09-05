using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Business
{
    public interface ILogManager
    {
        void Log<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType, Definitions.LogType logType = Definitions.LogType.NoTrace, string traceMessage = null);
        Task LogAsync<T>(string deviceIpAddress, string rlmSerial, T message, Definitions.LogMessageType logMessageType, Definitions.LogType logType = Definitions.LogType.NoTrace, string traceMessage = null);
        void TraceIt(Definitions.LogType logType, string message);
    }
}