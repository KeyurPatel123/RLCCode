using System.Threading.Tasks;
using Abiomed.DotNetCore.Models;

namespace Abiomed.DotNetCore.Mail
{
    public interface IMail
    {
        Task SendEmailAsync(string jsonMessage);
        Task SendEmailAsync(Email email);
        Task SendEmailAsync(string to, string subject, string body, string toFriendlyName, string fromEmail, string fromFriendlyName);
    }
}
