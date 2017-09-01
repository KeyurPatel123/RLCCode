using Abiomed.DotNetCore.Storage;

using System.Threading.Tasks;
using System;
using System.Threading;


namespace Abiomed.DotNetCore.MailConsole
{
    class Program
    {
        static private Mail _mail;
        static string _storagePath = string.Empty;
        static string _queueName = string.Empty;
        static int _pollingInterval = 0;
        static IQueueStorage _queueStorage;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Queue = Email");

            Initialize();
            while (true)
            {
                Task.Run(async () =>
                {
                    bool processMessages = true;

                    while (processMessages)
                    {
                        await _queueStorage.RetrieveMessageAsync();
                        string emailContent = _queueStorage.GetMessageContent();
                        if (string.IsNullOrWhiteSpace(emailContent))
                        {
                            processMessages = false;
                        }
                        else
                        { 
                            await _mail.SendEmailAsync(emailContent);
                            await _queueStorage.DeleteRetrievedMessageAsync();
                            processMessages = true;
                            // TODO: Add Logging when Email is Sent.
                        }
                    }
                }).GetAwaiter().GetResult();
                Thread.Sleep(_pollingInterval);
            }
        }

        static private void Initialize()
        {
            // TODO - When Convert to .NetCore 2.0 Add to appsettings.json and read from that.
            _queueName = "emails";
            _storagePath = "DefaultEndpointsProtocol=https;AccountName=remotelink;AccountKey=ykKtbMsrZJI8DvikFLhWgy7EpGheIfUJzKB87nTgoQm0hwLcYBYhnMxEJhcD+HIHMZ/bBvSf9kjHNg+4CnYd4w==";
            _mail = new Mail("NoReply_Abiomed@outlook.com", "No Reply", "www.abiomed.com", "plain", "USDVREX01.abiomed.com", 25);
            _queueStorage = new QueueStorage(_storagePath, _queueName);
            _pollingInterval = 60000;
        }
    }
}