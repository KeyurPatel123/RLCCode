using System;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Business
{
    public interface IAuditLogManager
    { 
        Task AuditAsync(string userName, DateTime logTime, string ipAddress, string action, string message);
    }
}