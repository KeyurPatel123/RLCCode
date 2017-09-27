using System;
using Abiomed.DotNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Abiomed.DotNetCore.Configuration
{
    public class ConfigurationCache : IConfigurationCache
    {
        #region Member Variables

        private const string _featureNameCannotBeNullEmptyOrWhitespace = "Feature Name cannot be null, empty, or whitespace.";
        private const string _keyNameCannotBeNullEmptyOrWhitespace = "Key Name cannot be null, empty, or whitespace.";
        private const string _valueCannotBeNullEmptyOrWhitespace = "Value cannot be null, empty, or whitespace.";
        private const string _keyNotFound = "Key not found - value could not be trieved from configurationCache.";
        private const string _couldNotConvertToInt = "Configuration Setting {0} : {1} with Value {2} is not numeric.";
        private const string _couldNotConvertToBool = "Configuration Setting {0} : {1} with Value {2} is not boolean.";

        private List<ConfigurationSetting> _configurationSettings;
        private IConfigurationManager _configurationManager;

        #endregion

        #region Constructors

        public ConfigurationCache(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager; 
        }

        #endregion

        #region Public Methods

        public async Task LoadCache()
        {
            _configurationSettings = new List<ConfigurationSetting>();

            if (_configurationManager.TableContext != "LOCAL_ONLY")
            { 
                await LoadSettingsFromCloudAsync();
            }

            OverrideSettings();
        }

        public string GetConfigurationItem(string featureName, string keyName)
        {
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

           return configurationSetting.Value;
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

            string value = GetConfigurationItem(featureName, keyName);;
            if (!int.TryParse(value, out int result))
            {
                throw new InvalidCastException(string.Format(_couldNotConvertToInt, featureName, keyName, value));
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

            string value = GetConfigurationItem(featureName, keyName);
            if (!bool.TryParse(value, out bool result))
            {
                throw new InvalidCastException(string.Format(_couldNotConvertToBool, featureName, keyName, value));
            }

            return result;
        }

        public void AddItemToCache(string category, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentOutOfRangeException(_featureNameCannotBeNullEmptyOrWhitespace);
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException(_keyNameCannotBeNullEmptyOrWhitespace);
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentOutOfRangeException(_valueCannotBeNullEmptyOrWhitespace);
            }

            ConfigurationSetting configurationSetting = new ConfigurationSetting
            {
                Category = category,
                Name = name,
                Value = value
            };
            _configurationSettings.Add(configurationSetting);

        }

        #endregion

        #region Private Methods

        private async Task LoadSettingsFromCloudAsync()
        {
            List<ApplicationConfiguration> applicationConfigurations = await _configurationManager.GetAllAsync();
            foreach (ApplicationConfiguration applicationConfiguration in applicationConfigurations)
            {
                if (applicationConfiguration.Active)
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
        }

        private void OverrideSettings()
        {
            var settingsToOverride = new List<ConfigurationSetting>();

            string path = Directory.GetCurrentDirectory() + @"\overrideappsettings.json";

            if (File.Exists(path))
            {
                JArray settings = JArray.Parse(File.ReadAllText(path));
                IList<ConfigurationSetting> configurationSettings = settings.Select(p => new ConfigurationSetting
                {
                    Category = (string)p["Category"],
                    Name = (string)p["Name"],
                    Value = (string)p["Value"]
                }).ToList();

                foreach (var setting in configurationSettings)
                {
                    ConfigurationSetting configurationSetting = _configurationSettings.Find(x => x.Category == setting.Category && x.Name == setting.Name);

                    if (configurationSetting != null)
                    {
                        _configurationSettings.Remove(configurationSetting);
                    }
                    _configurationSettings.Add(setting);
                }
            }
        }

        #endregion
    }
}