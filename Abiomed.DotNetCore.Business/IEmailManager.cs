using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Business
{
    public interface IEmailManager
    {
        void Listen();
        Task ListenToQueueStorageAsync();
        Task BroadcastAsync(string to, string subject, string body, string toFriendlyName = "", string from = "", string fromFriendlyName = "");
        Task BroadcastToQueueStorageAsync(string to, string subject, string body, string toFriendlyName = "", string from = "", string fromFriendlyName = "");
        Task StopAsync();
    }
}