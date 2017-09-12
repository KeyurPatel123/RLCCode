using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
using System;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Storage;

namespace Abiomed.DotNetCore.Test.EmailSendConsole
{
    class Program
    {
        private static IEmailManager _emailManager;

        static void Main(string[] args)
        {
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
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            IConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            await configurationCache.LoadCache();

            configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.ServiceBus.ToString());
            configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Broadcaster.ToString());

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, configurationCache), configurationCache);
        }

    }
}