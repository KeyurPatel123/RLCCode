using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abiomed.Models;

namespace Abiomed.DotNetCore
{
    public interface IMail
    {
        Task SendEmailAsync(string jsonMessage);
        Task SendEmailAsync(Email email);
        Task SendEmailAsync(string to, string subject, string body, string toFriendlyName, string fromEmail, string fromFriendlyName);
    }
}
