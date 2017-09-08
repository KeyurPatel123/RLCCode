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

        static private string _queueName = string.Empty;
        static private string _connection = string.Empty;
        static private string _fromEmail = string.Empty;
        static private string _fromFriendlyname = string.Empty;
        static private string _localDomainName = string.Empty;
        static private string _textPart = string.Empty;
        static private string _smtpHostName = string.Empty;
        static private int _portNumber = 0;

        private static IConfigurationManager _configurationManager { get; set; }

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static private async Task MainAsync(string[] args)
        {
            Console.WriteLine("Starting Email Message Pump");
            await Initialize();
            _emailManager.Listen(_fromEmail, _fromFriendlyname, _localDomainName, _smtpHostName, _portNumber, _textPart);

            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.ReadKey();

            await _emailManager.Stop();
        }

        static private async Task Initialize()
        {
            ITableStorage tableStorage = new TableStorage();
            _configurationManager = new ConfigurationManager(tableStorage);

            _queueName = (await _configurationManager.GetItemAsync("smtpmanager", "queuename")).Value;
            _connection = (await _configurationManager.GetItemAsync("smtpmanager", "queueconnection")).Value;
            string auditLogName = (await _configurationManager.GetItemAsync("auditlogmanager", "tablename")).Value;
            AuditLogManager auditLogManager = new AuditLogManager(auditLogName);

            _fromEmail = (await _configurationManager.GetItemAsync("smtpmanager", "fromemail")).Value;
            _fromFriendlyname = (await _configurationManager.GetItemAsync("smtpmanager", "fromfriendlyname")).Value;
            _localDomainName = (await _configurationManager.GetItemAsync("smtpmanager", "localdomain")).Value;
            _textPart = (await _configurationManager.GetItemAsync("smtpmanager", "bodytexttype")).Value;
            _smtpHostName = (await _configurationManager.GetItemAsync("smtpmanager", "host")).Value;
            _portNumber = int.Parse((await _configurationManager.GetItemAsync("smtpmanager", "portnumber")).Value);

            _emailManager = new EmailManager(auditLogManager, _queueName, _connection, EmailServiceActor.Listener);
        }
    }
}