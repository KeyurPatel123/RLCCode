using Abiomed.DotNetCore.Business;
using System;
using System.Threading.Tasks;


namespace Abiomed.DotNetCore.Test.EmailSendConsole
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

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Initialize();
            await _emailManager.Broadcast("plemay@abiomed.com", "this is a test", "Some text Goes Here", "Test Using Service Bus", "easterbummy_abiomed@outlook.com", "Abiomed Admin");
                

            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.ReadKey();

            await _emailManager.Stop();
        }


        private static void Initialize()
        {
            // TODO - When Convert to .NetCore 2.0 Add to appsettings.json and read from that.
            _queueName = "email";
            _connection = "Endpoint=sb://remotelinkmessagebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=R9mtWNYmXF+4LtmZkrZAqFUT1U1FQxGkE5gvQGctntc=";
          
            _emailManager = new EmailManager(new AuditLogManager("abiomedauditlog", "DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w=="), _queueName, _connection);
        }

    }
}