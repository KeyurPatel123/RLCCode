using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.Models;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;
using Abiomed.DotNetCore.Repository;
using Abiomed.DotNetCore.Business;

namespace Abiomed.DotNetCore.FactoryData
{
    class Program
    {
        private static IConfigurationManager _configurationManager { get; set; }
        private static string _factoryConfigurationFileName = "FactoryConfigurationSettings.json";
        private static string _institutionsConfigurationFileName = "Institutions.json";
        static IAzureCosmosDB _azureCosmosDB;
        static IConfigurationCache _configurationCache;
        static IConfigurationRoot _appConfiguration;
        static bool _isLoadInstitutions;
        static bool _isLoadConfigurations;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static private async Task MainAsync(string[] args)
        {
            await InitializeAsync();

            if (_isLoadConfigurations)
            {
                await LoadFactoryConfigurationAsync();
            }

            if (_isLoadInstitutions)
            {
                await LoadInstitutionsAsync();
            }
        }

        private static async Task InitializeAsync()
        {
            ITableStorage tableStorage = new TableStorage();
            _configurationManager = new ConfigurationManager(tableStorage);
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            _appConfiguration = builder.Build();
            _configurationCache = new ConfigurationCache(_configurationManager);
            await _configurationCache.LoadCacheAsync();

            _azureCosmosDB = new AzureCosmosDB(_configurationCache);
            _azureCosmosDB.SetContext(_configurationCache.GetConfigurationItem("azurecosmos", "coredatabasename"),
                _configurationCache.GetConfigurationItem("azurecosmos", "institutioncollectionname"));

            bool.TryParse(_appConfiguration.GetSection("LoadInstitutions").Value, out _isLoadInstitutions);
            bool.TryParse(_appConfiguration.GetSection("LoadConfigurations").Value, out _isLoadConfigurations);
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

        static private async Task LoadInstitutionsAsync()
        {
            string json = File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + _institutionsConfigurationFileName);
            JArray settings = JArray.Parse(json);
            IList<Institution> institutions = settings.Select(p => new Institution
            {
                SalesForceId = (string)p["SalesForceId"],
                SapCustomerId = (string)p["SapCustomerId"],
                DisplayName = (string)p["DisplayName"],
                Coordinate = new GeographicCoordinate
                {
                    Latitude = (string)p["GeographicCoordinate"]["Latitude"],
                    Longitude = (string)p["GeographicCoordinate"]["Longitude"]
                }

            }).ToList();

            foreach (Institution institution in institutions)
            {
                await _azureCosmosDB.AddAsync(institution);
            }
        }
    }
}