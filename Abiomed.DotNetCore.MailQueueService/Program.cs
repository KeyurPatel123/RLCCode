﻿using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Storage;

namespace Abiomed.DotNetCore.MailQueueService
{
    class Program
    {
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

            _pollingInterval = configurationCache.GetNumericConfigurationItem("smtpmanager", "pollinginterval");
            configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.Queue.ToString());
            configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Listener.ToString());

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, configurationCache), configurationCache);
        }
    }
}