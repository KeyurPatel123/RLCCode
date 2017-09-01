using Abiomed.DotNetCore.Business;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore.MailQueueService
{
    class Program
    {
        static string _storagePath = string.Empty;
        static string _queueName = string.Empty;
        static string _from = string.Empty;
        static string _fromFriendlyName = string.Empty;
        static string _localDomain = string.Empty;
        static string _textPart = string.Empty;
        static string _hostName = string.Empty;
        static int _port = 0;

        static int _pollingInterval = 0;
        static IEmailManager _emailManager;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Queue = Email");

            Initialize();
            while (true)
            {
                Task.Run(async () =>
                {
                    await _emailManager.ListenToQueueStorage(_from, _fromFriendlyName, _localDomain, _hostName, _port, _textPart);
                }).GetAwaiter().GetResult();
                Thread.Sleep(_pollingInterval);
            }
        }

        static private void Initialize()
        {
            // TODO - When Convert to .NetCore 2.0 Add to appsettings.json and read from that.
            _queueName = "email";
            _storagePath = "DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w==";
            _pollingInterval = 5000;
            _from = "NoReply_Abiomed@outlook.com";
            _fromFriendlyName = "No Reply";
            _localDomain = "www.abiomed.com";
            _textPart = "plain";
            _hostName = "USDVREX01.abiomed.com";
            _port = 25;

            _emailManager = new EmailManager(new AuditLogManager("abiomedauditlog", "DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w=="), _storagePath, _queueName);
        }
    }
}