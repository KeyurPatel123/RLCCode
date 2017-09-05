using Microsoft.WindowsAzure.Storage.Table;

namespace Abiomed.DotNetCore.Configuration
{
    class MetadataConfiguration : TableEntity
    {
        public string Description { get; set; }
        public string RowsLastUpdated { get; set; }
        public string SchemaLastUpdated { get; set; }
    }
}
