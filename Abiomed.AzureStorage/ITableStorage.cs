using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Storage
{
    public interface ITableStorage
    {
        Task<T> GetItemAsync<T>(string partitionKey, string rowKey, string tableName) where T : ITableEntity, new();
        Task<List<T>> GetPartitionItemsAsync<T>(string partitionKey, string tableName) where T : ITableEntity, new();
        
        Task<List<T>> GetAllAsync<T>(string tableName) where T : ITableEntity, new();
        
        Task<TableResult> InsertOrMergeAsync(string tableName, TableEntity tableEntity);
        
        Task<TableResult> UpdateAsync(string tableName, TableEntity tableEntity);
        
        Task<TableResult> InsertAsync(string tableName, TableEntity tableEntity);
      
        Task<TableResult> DeleteAsync(string tableName, TableEntity tableEntity);
       
        Task DropAsync();
        Task DropAsync(string tableName);
        Task SetTableContextAsync(string tableName);
    }
}
