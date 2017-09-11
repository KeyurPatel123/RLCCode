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

        static private string _fromEmail = string.Empty;
        static private string _fromFriendlyname = string.Empty;
        static private string _localDomainName = string.Empty;
        static private string _textPart = string.Empty;
        static private string _smtpHostName = string.Empty;
        static private int _portNumber = 0;

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
            ConfigurationManager configurationManager = new ConfigurationManager(tableStorage);
            IConfigurationCache configurationCache = new ConfigurationCache(configurationManager);
            await configurationCache.LoadCache();

            string queueName = configurationCache.GetConfigurationItem("smtpmanager", "queuename");
            string connection = configurationCache.GetConfigurationItem("smtpmanager", "queueconnection");

            _fromEmail = configurationCache.GetConfigurationItem("smtpmanager", "fromemail");
            _fromFriendlyname = configurationCache.GetConfigurationItem("smtpmanager", "fromfriendlyname");
            _localDomainName = configurationCache.GetConfigurationItem("smtpmanager", "localdomain");
            _textPart = configurationCache.GetConfigurationItem("smtpmanager", "bodytexttype");
            _smtpHostName = configurationCache.GetConfigurationItem("smtpmanager", "host");
            _portNumber = configurationCache.GetNumericConfigurationItem("smtpmanager", "portnumber");

            _emailManager = new EmailManager(new AuditLogManager(tableStorage, configurationCache), queueName, connection, EmailServiceActor.Listener);
        }
    }
}