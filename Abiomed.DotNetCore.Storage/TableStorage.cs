using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;  
using Microsoft.WindowsAzure.Storage.Table;  
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Abiomed.DotNetCore.Storage
{
    public class TableStorage : ITableStorage
    {
        #region Private Member Variables

        private const string tableContextCannotBeNull = @"Table Context cannot be null, empty, or whitespace.";
        private const string tableEntityCannotBeNull = @"TableEntity cannot be null.";
        private const string connectionStringCannotbeNull = @"Connection String cannot be null, empty, or whitespace.";

        private CloudTableClient _tableClient = null;
        private CloudTable _table = null;

        private IConfigurationRoot _configuration { get; set; }

        #endregion 

        #region Constructors

        /// <summary>
        /// Constructor 
        /// </summary>
        public TableStorage()
        {

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            _configuration = builder.Build();

            Initialize(_configuration.GetSection("AzureAbiomedCloud:StorageConnection").Value);
        }

        /// <summary>
        /// Shared Constructor Initialization Code
        /// </summary>
        private void Initialize(string storageConnection)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
            _tableClient = storageAccount.CreateCloudTableClient();
        }

        #endregion

        #region Public Methods

        #region Get Methods

        /// <summary>
        /// Gets a Specific Item from trhe Azure Cloud Table Storage (ASYNC)
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="partitionKey">The Feature (Primary Key)</param>
        /// <param name="rowKey">The Secondary Row Key within Feature</param>
        /// <param name="tableName">The Table Name to Query</param>
        /// <returns>the Object From storage</returns>
        public async Task<T> GetItemAsync<T>(string partitionKey, string rowKey, string tableName) where T : ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException(tableContextCannotBeNull);
            }

           await SetTableContextAsync(tableName);

            TableQuery<T> query = new TableQuery<T>().Where(
                        TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey))).Take(1);
            var results = new T();
            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);

                results = queryResult.FirstOrDefault();
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        /// <summary>
        /// Gets all the Entries belongiong to the specified Partition
        /// </summary>
        /// <typeparam name="T">Object Entity Type</typeparam>
        /// <param name="partitionKey">The Feature (Primary) key</param>
        /// <param name="tableName">The Table to be queried</param>
        /// <returns>List of objects entities</returns>
        public async Task<List<T>> GetPartitionItemsAsync<T>(string partitionKey, string tableName) where T : ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException(tableContextCannotBeNull);
            }

            await SetTableContextAsync(tableName);

            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);

                results = queryResult.Results;
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        /// <summary>
        /// Get all the configuration items for the specified table (Async)
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="tableName">The Table To retrieve the data from</param>
        /// <returns>List of Object Entities</returns>
        public async Task<List<T>> GetAllAsync<T>(string tableName) where T : ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException(tableContextCannotBeNull);
            }

            await SetTableContextAsync(tableName);

            TableQuery<T> query = new TableQuery<T>();
            var results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);

                results = queryResult.Results;
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        /// <summary>
        /// Gets a Single item (Async)
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="partitionKey">Primary Key</param>
        /// <param name="rowKey">Secondary Key</param>
        /// <returns>TableResults</returns>
        public async Task<TableResult> GetSingleAsync<T>(string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var tableResult = await _table.ExecuteAsync(retrieveOperation);
            return tableResult;
        }

        #endregion

        #region Store (aka save) Operations

        /// <summary>
        /// ASYNC
        /// Performs an Insert if the item does not exist or updates the one that exists based on PartitionKey and RowKey
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableEntity"></param>
        /// <returns></returns>
        public virtual async Task<TableResult> InsertOrMergeAsync(string tableName, TableEntity tableEntity)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(tableContextCannotBeNull);
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException(tableEntityCannotBeNull);
            }

            await SetTableContextAsync(tableName);
            return (await _table.ExecuteAsync(TableOperation.InsertOrMerge(tableEntity)));
        }

        /// <summary>
        /// ASYNC
        /// Updates the Entry in Table Storage
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableEntity"></param>
        /// <returns></returns>
        public virtual async Task<TableResult> UpdateAsync(string tableName, TableEntity tableEntity)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(tableContextCannotBeNull);
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException(tableEntityCannotBeNull);
            }

            await SetTableContextAsync(tableName);

            TableResult results = new TableResult();
            var itemExists = await GetSingleAsync<TableEntity>(tableEntity.PartitionKey, tableEntity.RowKey);
            if (itemExists != null && itemExists.Result != null)
            {
                results = await _table.ExecuteAsync(TableOperation.InsertOrMerge(tableEntity));
            }
            return results;
        }

        /// <summary>
        /// ASYNC
        /// Deletes the entry from table storage
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableEntity"></param>
        /// <returns></returns>
        public virtual async Task<TableResult> DeleteAsync(string tableName, TableEntity tableEntity)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(tableContextCannotBeNull);
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException(tableEntityCannotBeNull);
            }

            await SetTableContextAsync(tableName);

            TableResult results = new TableResult();
            var itemExists = await GetSingleAsync<TableEntity>(tableEntity.PartitionKey, tableEntity.RowKey);
            if (itemExists != null && itemExists.Result != null)
            {
                tableEntity.ETag = "*";
                results = await _table.ExecuteAsync(TableOperation.Delete(tableEntity));
            }
            return results;
        }

        /// <summary>
        /// Insert Operation - Async
        /// </summary>
        /// <param name="tableName">The name of the Table COntext for the Insert Operation</param>
        /// <param name="tableEntity">The TableEntity for the Insert Operation</param>
        /// <returns>Result Status from Operation</returns>
        public async Task<TableResult> InsertAsync(string tableName, TableEntity tableEntity)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(tableContextCannotBeNull);
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException(tableEntityCannotBeNull);
            }

            await SetTableContextAsync(tableName);
            return (await _table.ExecuteAsync(TableOperation.Insert(tableEntity)));
        }

        #endregion

        /// <summary>
        /// Drops a particular Table and allows for a table to be specified.
        /// </summary>
        /// <param name="tableName">The Table Name to be dropped</param>
        /// <returns></returns>
        public async Task DropAsync(string tableName)
        {
            await SetTableContextAsync(tableName);
            await _table.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Drops the partiicular table being used.
        /// </summary>
        /// <returns></returns>
        public async Task DropAsync()
        {
            await _table.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Creates a New Table if it does not exist and sets the operation context to the specified table.
        /// </summary>
        /// <param name="tableName">Table Name to set context</param>
        public async Task SetTableContextAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException(tableContextCannotBeNull);
            }

            if (_table == null || _table.Name != tableName)
            {
                _table = _tableClient.GetTableReference(tableName);
                await _table.CreateIfNotExistsAsync();
            }
        }
        
        #endregion
    }
}