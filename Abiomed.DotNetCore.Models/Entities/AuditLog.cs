using Microsoft.WindowsAzure.Storage.Table;

namespace Abiomed.DotNetCore.Models
{
    public class AuditLog : TableEntity
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}