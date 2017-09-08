using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Configuration
{
    public interface IConfigurationCache
    {
        Task LoadCache();
        void GetConfigurationItem(string featureName, string keyName, out string value);
        int GetNumericConfigurationItem(string featureName, string keyName);
        bool GetBooleanConfigurationItem(string featureName, string keyName);
    }
}
