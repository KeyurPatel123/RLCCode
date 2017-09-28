using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Configuration
{
    public interface IConfigurationCache
    {
        Task LoadCacheAsync();
        string GetConfigurationItem(string featureName, string keyName);
        int GetNumericConfigurationItem(string featureName, string keyName);
        bool GetBooleanConfigurationItem(string featureName, string keyName);
        void AddItemToCache(string category, string name, string value);
    }
}
