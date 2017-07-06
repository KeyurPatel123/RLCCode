using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using System.Collections.Generic;

namespace Abiomed.Storage
{
    /// <summary>
    /// Class to support Azure Table Storage
    /// </summary>
    public class TableStorage : ITableStorage
    {
        #region Private Member Variables

        private CloudTableClient _tableClient = null;
        private CloudTable _table = null;
        private CloudStorageAccount _storageAccount = null;

        #endregion 

        #region Constructors

        /// <summary>
        /// Constructor (overload) that sets the Table Context
        /// </summary>
        /// <param name="tableName"></param>
        public TableStorage(string tableName)
        {
            Initialize();

            // Create the Table.
            SetTableContext(tableName);
        }

        /// <summary>
        /// Constructor that does not set the table context as it can be set later
        /// </summary>
        public TableStorage()
        {
            Initialize();
        }

        #endregion

        #region Public Methods

        #region Get Methods

        /// <summary>
        /// Gets a Specific Item from trhe Azure Cloud Table Storage
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
                throw new ArgumentOutOfRangeException("Table Context cannot be null, empty, or whitespace.");
            }

            SetTableContext(tableName);

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
                throw new ArgumentOutOfRangeException("Table Context cannot be null, empty, or whitespace.");
            }

            SetTableContext(tableName);

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
        /// Get all the configuration items for the specified table
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="tableName">The Table To retrieve the data from</param>
        /// <returns>List of Object Entities</returns>
        public async Task<List<T>> GetAllAsync<T>(string tableName) where T : ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentOutOfRangeException("Table Context cannot be null, empty, or whitespace.");
            }

            SetTableContext(tableName);

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

        #endregion

        #region Store Operations

        /// <summary>
        /// Performs an Insert if the item does not exist or updates the one that exists based on PartitionKey and RowKey
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableEntity"></param>
        /// <returns></returns>
        public virtual async Task<TableResult> InsertOrMergeAsync(string tableName, TableEntity tableEntity)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("TableName cannot be null, empty, or whitespace.");
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException("TableEntity cannot be null.");
            }

            SetTableContext(tableName);
            return (await _table.ExecuteAsync(TableOperation.InsertOrMerge(tableEntity)));
        }

   /// <summary>
   /// Updates the Entry in Table Storage
   /// </summary>
   /// <param name="tableName"></param>
   /// <param name="tableEntity"></param>
   /// <returns></returns>
        public virtual async Task<TableResult> UpdateAsync(string tableName, TableEntity tableEntity) 
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("TableName cannot be null, empty, or whitespace.");
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException("TableEntity cannot be null.");
            }

            SetTableContext(tableName);

            TableResult results = new TableResult();
            var itemExists = await GetSingle<TableEntity>(tableEntity.PartitionKey, tableEntity.RowKey);
            if (itemExists != null && itemExists.Result != null)
            {
                results = await _table.ExecuteAsync(TableOperation.InsertOrMerge(tableEntity));
            }
            return results;
        }

        /// <summary>
        /// Deletes the entry from table storage
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableEntity"></param>
        /// <returns></returns>
        public virtual async Task<TableResult> DeleteAsync(string tableName, TableEntity tableEntity)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("TableName cannot be null, empty, or whitespace.");
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException("TableEntity cannot be null.");
            }

            SetTableContext(tableName);

            TableResult results = new TableResult();
            var itemExists = await GetSingle<TableEntity>(tableEntity.PartitionKey, tableEntity.RowKey);
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
                throw new ArgumentNullException("TableName cannot be null, empty, or whitespace.");
            }

            if (tableEntity == null)
            {
                throw new ArgumentNullException("TableEntity cannot be null.");
            }

            SetTableContext(tableName);
            return (await _table.ExecuteAsync(TableOperation.Insert(tableEntity)));
        }
        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Shared Constructor Initialization Code
        /// </summary>
        private void Initialize()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            _tableClient = _storageAccount.CreateCloudTableClient();
        }

        /// <summary>
        /// Creates a New Table if it does not exist and sets the operation context to the specified table.
        /// </summary>
        /// <param name="tableName">Table Name to set context</param>
        private void SetTableContext(string tableName)
        {
            _table = _tableClient.GetTableReference(tableName);
            _table.CreateIfNotExists();
        }

        /// <summary>
        /// Gets a Single item 
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="partitionKey">Primary Key</param>
        /// <param name="rowKey">Secondary Key</param>
        /// <returns>TableResults</returns>
        public async Task<TableResult> GetSingle<T>(string partitionKey, string rowKey) where T :ITableEntity, new()
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var tableResult = await _table.ExecuteAsync(retrieveOperation);
            return tableResult; 
        }

        #endregion
    }
}