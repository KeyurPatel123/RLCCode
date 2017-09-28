using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Repository 
{
    public interface IAzureCosmosDB
    {
        Task AddAsync<T>(T payload);
        void SetContext(string databaseName, string collectionName);
    }
}