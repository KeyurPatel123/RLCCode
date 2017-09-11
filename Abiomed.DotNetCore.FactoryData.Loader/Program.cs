using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Abiomed.DotNetCore.FactoryData
{
    class Program
    {
        private static IConfigurationManager _configurationManager { get; set; }
        private static string _factoryConfigurationFileName = "FactoryConfigurationSettings.json";

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static private async Task MainAsync(string[] args)
        {
            ITableStorage tableStorage = new TableStorage();
            _configurationManager = new ConfigurationManager(tableStorage);

            await LoadFactoryConfigurationAsync();
        }

        static private async Task LoadFactoryConfigurationAsync()
        {
            string json = File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + _factoryConfigurationFileName);
            JArray settings = JArray.Parse(json);
            IList<ConfigurationSetting> configurationSettings = settings.Select(p => new ConfigurationSetting
            {
                Category = (string)p["Category"],
                Name = (string)p["Name"],
                Value = (string)p["Value"]
            }).ToList();

            foreach(var setting in configurationSettings)
            {
                await _configurationManager.AddConfigurationItemAsync(setting.Category, setting.Name, setting.Value);
            }
        }
    }
}
