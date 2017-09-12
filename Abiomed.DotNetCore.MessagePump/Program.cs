using System;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Business;
using Microsoft.Extensions.Configuration;
using System.IO;
using Abiomed.DotNetCore.Storage;
using Abiomed.DotNetCore.Configuration;

namespace Abiomed.DotNetCore.MessagePump
{
    class Program
    {
        static private IEmailManager _emailManager;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static private async Task MainAsync(string[] args)
        {
            Console.WriteLine("Starting Email Message Pump");
            await Initialize();
            _emailManager.Listen();

            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.ReadKey();

            await _emailManager.Stop();
        }

        static private async Task Initialize()
        {
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            IConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            await configurationCache.LoadCache();

            configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.ServiceBus.ToString());
            configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Listener.ToString());

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, configurationCache), configurationCache);
        }
    }
}