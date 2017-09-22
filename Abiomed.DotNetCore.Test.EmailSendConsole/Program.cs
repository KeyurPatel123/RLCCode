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
        private static IConfigurationCache _configurationCache;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            await Initialize();

            if (Enum.TryParse(_configurationCache.GetConfigurationItem("smtpmanager", "emailservicetype"), out EmailServiceType emailServiceType))
            {

                if (emailServiceType == EmailServiceType.ServiceBus)
                {
                    await _emailManager.Broadcast("plemay@abiomed.com", "this is a test", "Some text Goes Here", "Test Using Service Bus", "easterbunny_abiomed@outlook.com", "Abiomed Admin");
                }
                else
                {
                    await _emailManager.BroadcastToQueueStorage("plemay@abiomed.com", "this is a test", "Some text Goes Here", "Lemay, Paolo");
                }

                Console.WriteLine("Press any key to exit after receiving all the messages.");
                Console.ReadKey();

                await _emailManager.Stop();
            }
            else
            {
                Console.WriteLine("Error retrieving email manager settings.");
            }
        }

        private static async Task Initialize()
        {
            ITableStorage tableStorage = new TableStorage();
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            _configurationCache = new ConfigurationCache(configurationManager);
            await _configurationCache.LoadCache();

            _configurationCache.AddItemToCache("smtpmanager", "emailservicetype", EmailServiceType.ServiceBus.ToString());
            _configurationCache.AddItemToCache("smtpmanager", "emailserviceactor", EmailServiceActor.Broadcaster.ToString());

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, _configurationCache), _configurationCache);
        }

    }
}