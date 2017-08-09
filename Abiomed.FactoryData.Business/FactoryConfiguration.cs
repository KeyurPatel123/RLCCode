using System.Threading.Tasks;
using Abiomed.Configuration;

namespace Abiomed.FactoryData
{
    public class FactoryConfiguration : IFactoryConfiguration
    {
        private ConfigurationManager _configurationManager;
        private HospitalManager _hospitalManager;
        private static string _configurationSettingsTableName = @"config";
        private static string _hospitals= @"hospitals";

        public FactoryConfiguration()
        {
            Initialize();
        }

        public async Task SetFactoryData(bool isCloud = false)
        {
            await _configurationManager.LoadFactoryConfiguration();
            await _hospitalManager.LoadHospitalFactoryDataAsync();
        }

        private void Initialize()
        {
            _configurationManager = new ConfigurationManager(_configurationSettingsTableName);
            _hospitalManager = new HospitalManager(_hospitals);
        }
    }
}