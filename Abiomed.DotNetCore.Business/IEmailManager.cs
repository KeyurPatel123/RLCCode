using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Business
{
    public interface IEmailManager
    {
        void Listen(string fromEmail, string fromFriendlyName, string localDomainName, string smtpHostName, int smtpPortNumber, string textPart = "plain");
        Task ListenToQueueStorage(string fromEmail, string fromFriendlyName, string localDomainName, string smtpHostName, int smtpPortNumber, string textPart = "plain");
        Task Broadcast(string to, string subject, string body, string toFriendlyName = "", string from = "", string fromFriendlyName = "");
        Task BroadcastToQueueStorage(string to, string subject, string body, string toFriendlyName = "", string from = "", string fromFriendlyName = "");
        Task Stop();
    }
}