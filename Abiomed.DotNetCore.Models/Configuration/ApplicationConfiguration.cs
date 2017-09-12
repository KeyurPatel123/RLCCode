using Microsoft.WindowsAzure.Storage.Table;

namespace Abiomed.DotNetCore.Models
{
    public class ApplicationConfiguration : TableEntity
    {

        /// <summary>
        /// Gets or Sets the Value Property 
        /// </summary>
        public string Value { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public string DeactiveDate { get; set; } = string.Empty;
        public string DeactiveBy { get; set; } = string.Empty;
    }
}