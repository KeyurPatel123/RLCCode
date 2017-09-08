using Abiomed.DotNetCore.Business;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Abiomed.DotNetCore.Storage;

namespace Abiomed.DotNetCore.MailQueueService
{
    class Program
    {
        static string _from = string.Empty;
        static string _fromFriendlyName = string.Empty;
        static string _localDomain = string.Empty;
        static string _textPart = string.Empty;
        static string _hostName = string.Empty;
        static int _port = 0;

        static int _pollingInterval = 0;
        static IEmailManager _emailManager;
        private static IConfigurationRoot _configuration { get; set; }
        private static IConfigurationManager _configurationManager { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Queue = Email");

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            Initialize().Wait();

            while (true)
            {
                Task.Run(async () =>
                {
                    await _emailManager.ListenToQueueStorage(_from, _fromFriendlyName, _localDomain, _hostName, _port, _textPart);
                }).GetAwaiter().GetResult();
                Thread.Sleep(_pollingInterval);
            }
        }

        static private async Task Initialize()
        {
            string storageConnection = _configuration.GetSection("AzureAbiomedCloud:StorageConnection").Value;
            ITableStorage tableStorage = new TableStorage(storageConnection);
            _configurationManager = new ConfigurationManager(tableStorage);
            _configurationManager.SetTableContext(_configuration.GetSection("AzureAbiomedCloud:ConfigurationTableName").Value);
            string queueName = (await _configurationManager.GetItemAsync("smtpmanager", "queuename")).Value;
            _pollingInterval = int.Parse((await _configurationManager.GetItemAsync("smtpmanager", "pollinginterval")).Value);
            _from = (await _configurationManager.GetItemAsync("smtpmanager", "fromemail")).Value;
            _fromFriendlyName = (await _configurationManager.GetItemAsync("smtpmanager", "fromfriendlyname")).Value;
            _localDomain = (await _configurationManager.GetItemAsync("smtpmanager", "localdomain")).Value;
            _textPart = (await _configurationManager.GetItemAsync("smtpmanager", "bodytexttype")).Value;
            _hostName = (await _configurationManager.GetItemAsync("smtpmanager", "host")).Value;
            _port = int.Parse((await _configurationManager.GetItemAsync("smtpmanager", "portnumber")).Value);
            string auditLogName = (await _configurationManager.GetItemAsync("auditlogmanager", "tablename")).Value;
            AuditLogManager auditLogManager = new AuditLogManager(auditLogName, storageConnection);

            _emailManager = new EmailManager(auditLogManager, queueName, storageConnection);
        }
    }
}