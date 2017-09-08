using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Abiomed.DotNetCore.Configuration
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
    public class ConfigurationManager : IConfigurationManager
    {
        #region Private Member Variables

        private const string _configurationTableCannotBeNullEmptyOrWhitespace = "Configuration Table Context cannot be null, empty, or whitespace.";
        private const string _configurationItemsCannotBeNull = "Configuration Items cannot be null or empty list.";
        private const string _partitionKeyCannotBeNullEmptyOrWhitespace = "Partition Key cannot be null, empty, or whitespace.";
        private const string _rowKeyCannotBeNullEmptyOrWhitespace = "Row Key cannot be null, empty, or whitespace.";
        private const string _valueCannotBeNullEmptyOrWhitespace = "Value cannot be null, empty, or whitespace.";

        private ITableStorage _iTableStorage;
        private string _tableContext = string.Empty;

        private IConfigurationRoot _configuration { get; set; }

        #endregion

        #region Constructors

        public ConfigurationManager(ITableStorage tableStorage)
        {
            _iTableStorage = tableStorage;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            _tableContext = _configuration.GetSection("AzureAbiomedCloud:ConfigurationTableName").Value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes or Sets the Table to be used in the operation
        /// </summary>
        /// <param name="tableName">Table Name to use</param>
        public void SetTableContext(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException(_configurationTableCannotBeNullEmptyOrWhitespace);
            }

            _tableContext = tableName;
        }

        /// <summary>
        /// In Azure Terms this is a point query and known as the most efficient.
        /// </summary>
        /// <param name="featureKey">In Azure Terms this is the PartitionKey</param>
        /// <param name="itemKey">In Azure Terms this is the RowKey</param>
        /// <returns>ApplicationConfiguration Entry</returns>
        public async Task<ApplicationConfiguration> GetItemAsync(string featureKey, string itemKey)
        {
            return await _iTableStorage.GetItemAsync<ApplicationConfiguration>(featureKey, itemKey, _tableContext);
        }

        /// <summary>
        /// Gets a List of all settings for a given feature (or Partition)
        /// </summary>
        /// <param name="featureKey">In Azure Terms this is the PartitionKey</param>
        /// <returns>List of ApplicationConfiguration Entries</returns>
        public async Task<List<ApplicationConfiguration>> GetFeatureAsync(string featureKey)
        {
           return await _iTableStorage.GetPartitionItemsAsync<ApplicationConfiguration>(featureKey, _tableContext);
        }

        /// <summary>
        /// Gats all the items in the configuration table
        /// </summary>
        /// <returns>List of ApplicationConfiguration Entries</returns>
        public async Task<List<ApplicationConfiguration>> GetAllAsync()
        {
            return await _iTableStorage.GetAllAsync<ApplicationConfiguration>(_tableContext);
        }
        

        public async Task StoreConfigurationItemsAsync(List<ApplicationConfiguration> configurationItems, string configurationContext = null)
        {
            if (configurationItems == null)
            {
                throw new ArgumentNullException(_configurationItemsCannotBeNull);
            }

            if (configurationItems.Count == 0)
            {
                throw new ArgumentOutOfRangeException(_configurationItemsCannotBeNull);
            }

            if (!string.IsNullOrWhiteSpace(configurationContext))
            {
                _tableContext = configurationContext;
            }

            if (string.IsNullOrWhiteSpace(_tableContext))
            {
                throw new ArgumentOutOfRangeException(_configurationTableCannotBeNullEmptyOrWhitespace);
            }

            foreach (ApplicationConfiguration item in configurationItems)
            {
                await _iTableStorage.InsertAsync(_tableContext, item);
            }
        }

        /// <summary>
        /// Adds an item to the Configuation Table
        /// </summary>
        /// <param name="partitionKey">Feature</param>
        /// <param name="rowKey">Key name</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public async Task AddConfigurationItemAsync(string partitionKey, string rowKey, string value)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw new ArgumentOutOfRangeException(_partitionKeyCannotBeNullEmptyOrWhitespace);
            }

            if (string.IsNullOrWhiteSpace(rowKey))
            {
                throw new ArgumentOutOfRangeException(_rowKeyCannotBeNullEmptyOrWhitespace);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentOutOfRangeException(_valueCannotBeNullEmptyOrWhitespace);
            }

            ApplicationConfiguration appConfig = new ApplicationConfiguration
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Value = value
            };

            await _iTableStorage.InsertOrMergeAsync(_tableContext, appConfig);
        }

        #endregion
    }
}