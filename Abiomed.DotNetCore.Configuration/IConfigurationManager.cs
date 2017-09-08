using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Models;

namespace Abiomed.DotNetCore.Configuration
{
    public interface IConfigurationManager
    {
        void SetTableContext(string tableName);
        Task<ApplicationConfiguration> GetItemAsync(string featureKey, string itemKey);
        Task<List<ApplicationConfiguration>> GetFeatureAsync(string featureKey);
        Task<List<ApplicationConfiguration>> GetAllAsync();
        Task StoreConfigurationItemsAsync(List<ApplicationConfiguration> configurationItems, string configurationContext = null);
        Task AddConfigurationItemAsync(string partitionKey, string rowKey, string value);
    }
}