using Abiomed.Business;
using Abiomed.Models;
using System;
using SystemConfig = System.Configuration.ConfigurationManager;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Microsoft.Azure; // Namespace for CloudConfigurationManager

namespace Abiomed.FactoryData
{
    class Program
    {
        static ConfigurationManager _configurationManager;

        static void Main(string[] args)
        {
            Initialize();

            var x =  _configurationManager.GetItemAsync("remotelink", "logname");
            LoadFactoryConfigurationData();
        }

        static private void LoadFactoryConfigurationData()
        {
            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            applicationConfiguration.PartitionKey = "remotelink";
            applicationConfiguration.RowKey = "localhost";
            applicationConfiguration.Value = @"localhost";
            _configurationManager.InsertOrMergeConfigurationItemAsync(applicationConfiguration);
        }

        static private void Initialize()
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            string tableName = SystemConfig.AppSettings["ConfigurationTableName"].ToString();
            _configurationManager = new ConfigurationManager(tableName, cloudStorageAccount);
        }
    }
}
