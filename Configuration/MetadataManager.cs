using Abiomed.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Configuration
{
    class MetadataManager
    {
        private ITableStorage _iTableStorage;
        private string _tableContext;

        public MetadataManager()
        {
            SetTableContext("Metadata"); // TODO Need to set from somewhere instead of HardCode.
            Initialize();
        }

        public void SetTableContext(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException("Metadata Table Context cannot be null, empty, or whitespace.");
            }

            _tableContext = tableName;
        }

        public async Task AddMetadataAsync(string name, string description, string sourceRowsLastUpdatedDate = "", string SourceSchemaLastUpdated = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("Metadata Name cannot be null, empty, or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("Metadata Description cannot be null, empty, or whitespace.");
            }

            MetadataConfiguration metadata = new MetadataConfiguration();
            metadata.PartitionKey = "Configuration";
            metadata.RowKey = name;
            metadata.Description = description;
            metadata.RowsLastUpdated = sourceRowsLastUpdatedDate;
            metadata.SchemaLastUpdated = SourceSchemaLastUpdated;

            await _iTableStorage.InsertOrMergeAsync(_tableContext, metadata);
        }

        public MetadataConfiguration Get(string partitionKey, string rowKey)
        {
            return _iTableStorage.GetItem<MetadataConfiguration>(partitionKey, rowKey, _tableContext);
        }

        private void Initialize()
        {
            _iTableStorage = new TableStorage();
        }
    }
}
