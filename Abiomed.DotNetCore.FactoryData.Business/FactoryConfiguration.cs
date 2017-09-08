using System;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Business;

namespace Abiomed.DotNetCore.FactoryData
{
    public class FactoryConfiguration : IFactoryConfiguration
    {
        private IConfigurationManager _configurationManager;
        private static string _configurationSettingsTableName = @"config";

        public FactoryConfiguration(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public async Task SetFactoryData(bool isCloud = false)
        {
            await _configurationManager.LoadFactoryConfiguration();
        }
    }
}
