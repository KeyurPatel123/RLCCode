using Abiomed.DotNetCore.Business;
using Abiomed.DotNetCore.Configuration;
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

        private static IConfigurationManager _configurationManager { get; set; }

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
            _configurationManager = new ConfigurationManager(tableStorage);

            _queueName = (await _configurationManager.GetItemAsync("smtpmanager", "queuename")).Value;
            _connection = (await _configurationManager.GetItemAsync("smtpmanager", "queueconnection")).Value;
            string auditLogName = (await _configurationManager.GetItemAsync("auditlogmanager", "tablename")).Value;
            AuditLogManager auditLogManager = new AuditLogManager(auditLogName);
            _emailManager = new EmailManager(auditLogManager, _queueName, _connection);
        }

    }
}