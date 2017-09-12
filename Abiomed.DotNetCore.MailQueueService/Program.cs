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
                    await _emailManager.ListenToQueueStorage();
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

            configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.Queue.ToString());
            configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Listener.ToString());

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, configurationCache), configurationCache);
        }
    }
}