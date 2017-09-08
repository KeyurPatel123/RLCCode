//using Autofac;
//using Abiomed.DependencyInjection;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
            var serviceProvider = new ServiceCollection()
                                    .AddLogging()
                                    .AddSingleton<TableStorage>()
                                    .AddSingleton<ConfigurationManager>()
                                    .BuildServiceProvider();

           // serviceProvider
           //     .GetService<ILoggerFactory>().AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                                       .CreateLogger<Program>();

            logger.LogDebug("Starting Factory Configuration Load");
   
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
