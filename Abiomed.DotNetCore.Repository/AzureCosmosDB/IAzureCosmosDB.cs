using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Abiomed.DotNetCore.Repository 
{
    public interface IAzureCosmosDB
    {
        Task AddAsync<T>(T payload);
        void SetContext(string databaseName, string collectionName);
        List<T> ExecuteQuery<T>(string databaseName, string collectionName, string where);
        List<T> ExecuteQuery<T>(Uri documentCollectionUri, string collectionName, string where);
    }
}