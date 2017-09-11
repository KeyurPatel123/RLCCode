using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
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
        static string _connection = string.Empty;
        static int _port = 0;

        static int _pollingInterval = 0;
        static IEmailManager _emailManager;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Queue = Email");
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
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            IConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            await configurationCache.LoadCache();

            string queueName = configurationCache.GetConfigurationItem("smtpmanager", "queuename");
            _pollingInterval = configurationCache.GetNumericConfigurationItem("smtpmanager", "pollinginterval");
            _from = configurationCache.GetConfigurationItem("smtpmanager", "fromemail");
            _fromFriendlyName = configurationCache.GetConfigurationItem("smtpmanager", "fromfriendlyname");
            _localDomain = configurationCache.GetConfigurationItem("smtpmanager", "localdomain");
            _textPart = configurationCache.GetConfigurationItem("smtpmanager", "bodytexttype");
            _hostName = configurationCache.GetConfigurationItem("smtpmanager", "host");
            _port = configurationCache.GetNumericConfigurationItem("smtpmanager", "portnumber");
            _connection = configurationCache.GetConfigurationItem("smtpmanager", "queuestorage");

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, configurationCache), queueName, _connection);
        }
    }
}