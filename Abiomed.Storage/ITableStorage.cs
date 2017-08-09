using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abiomed.Storage
{
    public interface ITableStorage
    {
        Task<T> GetItemAsync<T>(string partitionKey, string rowKey, string tableName) where T : ITableEntity, new();
        T GetItem<T>(string partitionKey, string rowKey, string tableName) where T : ITableEntity, new();
        Task<List<T>> GetPartitionItemsAsync<T>(string partitionKey, string tableName) where T : ITableEntity, new();
        List<T> GetPartitionItems<T>(string partitionKey, string tableName) where T : ITableEntity, new();
        Task<List<T>> GetAllAsync<T>(string tableName) where T : ITableEntity, new();
        List<T> GetAll<T>(string tableName) where T : ITableEntity, new();
        Task<TableResult> InsertOrMergeAsync(string tableName, TableEntity tableEntity);
        TableResult InsertOrMerge(string tableName, TableEntity tableEntity);
        Task<TableResult> UpdateAsync(string tableName, TableEntity tableEntity);
        TableResult Update(string tableName, TableEntity tableEntity);
        Task<TableResult> InsertAsync(string tableName, TableEntity tableEntity);
        TableResult Insert(string tableName, TableEntity tableEntity);
        Task<TableResult> DeleteAsync(string tableName, TableEntity tableEntity);
        TableResult Delete(string tableName, TableEntity tableEntity);
        void Drop();
        void Drop(string tableName);
        Task DropAsync();
        Task DropAsync(string tableName);
    }
}