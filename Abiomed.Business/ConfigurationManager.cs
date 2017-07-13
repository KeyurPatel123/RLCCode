using Abiomed.Models;
using Abiomed.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Business
{
    /// <summary>
    /// Cloud Table Storage uses the following structure.
    /// 1) PartitionKey - Grouping of data
    /// 2) RowKey - Unique Key within Group
    /// 3) Additional Feilds can be added.
    /// 
    /// In context to Configuration...
    /// The construct implemented is...
    /// - PartitionKey is equivalent to the Feature (or application)
    /// - Rowkey is the Configuration Item Key within the Partition
    /// - Then Value
    /// </summary>
    public class ConfigurationManager
    {
        private TableStorage _tableStorage;
        private string _tableContext;

        public ConfigurationManager()
        {
            Initialize();
        }

        public ConfigurationManager(string tableContext)
        {
            SetTableContext(tableContext);
            Initialize();
        }

        public void SetTableContext(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException("Configuration Table Context cannot be null, empty, or whitespace.");
            }

            _tableContext = tableName;
        }

        /// <summary>
        /// In Azure Terms this is a point query and known as the most efficient.
        /// </summary>
        /// <param name="featureKey">In Azure Terms this is the PartitionKey</param>
        /// <param name="itemKey">In Azure Terms this is the RowKey</param>
        /// <returns></returns>
        public async Task<ApplicationConfiguration> GetItemAsync(string featureKey, string itemKey)
        {
            return await _tableStorage.GetItemAsync<ApplicationConfiguration>(featureKey, itemKey, _tableContext);
        }

        public async Task<List<ApplicationConfiguration>> GetFeatureAsync(string featureKey)
        {
           return await _tableStorage.GetPartitionItemsAsync<ApplicationConfiguration>(featureKey, _tableContext);
        }

        public async Task<List<ApplicationConfiguration>> GetAllAsync()
        {
            return await _tableStorage.GetAllAsync<ApplicationConfiguration>(_tableContext);
        }

        public async Task StoreConfigurationItemsAsync(List<ApplicationConfiguration> configurationItems, string configurationContext = null)
        {
            if (configurationItems == null)
            {
                throw new ArgumentNullException("ConfigurationItems cannot be null.");
            }

            if (configurationItems.Count == 0)
            {
                throw new ArgumentOutOfRangeException("ConfigurationItems cannot be an empty list.");
            }

            if (!string.IsNullOrWhiteSpace(configurationContext))
            {
                _tableContext = configurationContext;
            }

            if (string.IsNullOrWhiteSpace(_tableContext))
            {
                throw new ArgumentOutOfRangeException("Configuration Table Context cannot be null, empty, or whitespace.");
            }

            foreach (ApplicationConfiguration item in configurationItems)
            {
                await _tableStorage.InsertAsync(_tableContext, item);
            }
        }

        private void Initialize()
        {
            _tableStorage = new TableStorage();
        }
    }
}