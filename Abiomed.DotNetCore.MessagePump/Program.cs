﻿using System;
using System.Threading.Tasks;
using System.Threading;
using Abiomed.DotNetCore.Communication;
using Abiomed.DotNetCore.Business;

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

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static private async Task MainAsync(string[] args)
        {
            Console.WriteLine("Starting Email Message Pump");
            Initialize();
            _emailManager.Listen(_fromEmail, _fromFriendlyname, _localDomainName, _smtpHostName, _portNumber, _textPart);

            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.ReadKey();

            await _emailManager.Stop();
        }

        static private void Initialize()
        {
            // TODO - When Convert to .NetCore 2.0 Add to appsettings.json and read from that.
            _queueName = "email";
            _connection = "Endpoint=sb://remotelinkmessagebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=R9mtWNYmXF+4LtmZkrZAqFUT1U1FQxGkE5gvQGctntc=";
            _fromEmail = "santa_abiomed@outlook.com";
            _fromFriendlyname = "Abiomed Admin";
            _localDomainName = "www.abiomed.com";
            _textPart = "plain";
            _smtpHostName = "USDVREX01.abiomed.com";
            _portNumber = 25;

            _emailManager = new EmailManager(new AuditLogManager("abiomedauditlog", "DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w=="), _queueName, _connection, EmailServiceActor.Listener);
        }
    }
}