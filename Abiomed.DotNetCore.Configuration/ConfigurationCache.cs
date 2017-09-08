using System;
using Abiomed.DotNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.Configuration 
{
    public class ConfigurationCache : IConfigurationCache
    {
        private const string _featureNameCannotBeNullEmptyOrWhitespace = "Feature Name cannot be null, empty, or whitespace.";
        private const string _keyNameCannotBeNullEmptyOrWhitespace = "Key Name cannot be null, empty, or whitespace.";
        private const string _keyNotFound = "Key not found - value could not be trieved from configurationCache.";
        private const string _couldNotConvertToInt = "Value {0} retrieved from configuration Cache is not a numeric type.";
        private const string _couldNotConvertToBool = "Value {0} retrieved from configuration Cache is not a boolean type.";

        private List<ConfigurationSetting> _configurationSettings;
        private IConfigurationManager _configurationManager;

        public ConfigurationCache(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager; 
        }

        public async Task LoadCache()
        {
            _configurationSettings = new List<ConfigurationSetting>();
            List<ApplicationConfiguration> applicationConfigurations =  await _configurationManager.GetAllAsync();
            foreach(ApplicationConfiguration applicationConfiguration in applicationConfigurations)
            {
                ConfigurationSetting configurationSetting = new ConfigurationSetting
                {
                    Category = applicationConfiguration.PartitionKey,
                    Name = applicationConfiguration.RowKey,
                    Value = applicationConfiguration.Value
                };

                _configurationSettings.Add(configurationSetting);
            }
        }

        public void GetConfigurationItem(string featureName, string keyName, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentOutOfRangeException(_featureNameCannotBeNullEmptyOrWhitespace);
            }

            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentOutOfRangeException(_keyNameCannotBeNullEmptyOrWhitespace);
            }

            ConfigurationSetting configurationSetting = _configurationSettings.Find(x => x.Category == featureName && x.Name == keyName);

            if (configurationSetting == null)
            {
                throw new ArgumentOutOfRangeException(_keyNotFound);
            }

            value = configurationSetting.Value;
        }

        public int GetNumericConfigurationItem(string featureName, string keyName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentOutOfRangeException(_featureNameCannotBeNullEmptyOrWhitespace);
            }

            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentOutOfRangeException(_keyNameCannotBeNullEmptyOrWhitespace);
            }

            string value = string.Empty;
            GetConfigurationItem(featureName, keyName, out value);
            int result = int.MinValue;
            if (!int.TryParse(value, out result))
            {
                throw new InvalidCastException(string.Format(_couldNotConvertToInt, value));
            }

            return result;
        }

        public bool GetBooleanConfigurationItem(string featureName, string keyName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentOutOfRangeException(_featureNameCannotBeNullEmptyOrWhitespace);
            }

            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentOutOfRangeException(_keyNameCannotBeNullEmptyOrWhitespace);
            }

            string value = string.Empty;
            GetConfigurationItem(featureName, keyName, out value);
            bool result = false;
            if (!bool.TryParse(value, out result))
            {
                throw new InvalidCastException(string.Format(_couldNotConvertToBool, value));
            }

            return result;
        }

    }
}
