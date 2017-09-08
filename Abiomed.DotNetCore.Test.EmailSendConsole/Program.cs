using Abiomed.DotNetCore.Business;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Abiomed.DotNetCore.Storage;

namespace Abiomed.DotNetCore.Test.EmailSendConsole
{
    class Program
    {
        private static IEmailManager _emailManager;
        private static string _queueName = string.Empty;
        private static string _connection = string.Empty;

        private static IConfigurationRoot _configuration { get; set; }
        private static IConfigurationManager _configurationManager { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

            _configuration = builder.Build();

            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            await Initialize();
            await _emailManager.Broadcast("plemay@abiomed.com", "this is a test", "Some text Goes Here", "Test Using Service Bus", "easterbummy_abiomed@outlook.com", "Abiomed Admin");
                
            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.ReadKey();

            await _emailManager.Stop();
        }

        private static async Task Initialize()
        {
            string storageConnection = _configuration.GetSection("AzureAbiomedCloud:StorageConnection").Value;
            ITableStorage tableStorage = new TableStorage(storageConnection);
            _configurationManager = new ConfigurationManager(tableStorage);
            _configurationManager.SetTableContext(_configuration.GetSection("AzureAbiomedCloud:ConfigurationTableName").Value);

            _queueName = (await _configurationManager.GetItemAsync("smtpmanager", "queuename")).Value;
            _connection = (await _configurationManager.GetItemAsync("smtpmanager", "queueconnection")).Value;
            string auditLogName = (await _configurationManager.GetItemAsync("auditlogmanager", "tablename")).Value;
            AuditLogManager auditLogManager = new AuditLogManager(auditLogName, storageConnection);
            _emailManager = new EmailManager(auditLogManager, _queueName, _connection);
        }

    }
}