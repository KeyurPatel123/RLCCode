using Microsoft.WindowsAzure.Storage.Table;

namespace Abiomed.DotNetCore.Configuration
{
    public class ApplicationConfiguration : TableEntity
    {

        /// <summary>
        /// Gets or Sets the Value Property 
        /// </summary>
        public string Value { get; set; }
    }
}