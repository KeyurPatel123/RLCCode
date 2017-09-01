using System;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore.Communication
{

    public class ServiceBus : IServiceBus
    {
        private const string _messageCannotBeNull = "Message cannot be null.";
        private const string _queueNameCannotBeEmpty = "Queue Name cannot be null or empty.";
        private const string _connectionStringCannotBeEmpty = "Connection String cannot be null or empty";
        private const string _invalidReceiveMode = "Invalid Receive Mode";

        private IQueueClient _queueClient;

        public ServiceBus(string queueName, string connection)
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new ArgumentOutOfRangeException(_connectionStringCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentOutOfRangeException(_queueNameCannotBeEmpty);
            }

            _queueClient = new QueueClient(connection, queueName, ReceiveMode.PeekLock);
        }

        public async Task CloseAsync()
        {
            if (!_queueClient.IsClosedOrClosing)
            {
                await _queueClient.CloseAsync();
            }
        }

        public IQueueClient GetQueueClient()
        {
            return _queueClient;
        }

        public async Task SendMessageAsync<T>(T objectToAdd)
        {
            if (objectToAdd == null)
            {
                throw new ArgumentNullException(_messageCannotBeNull);
            }

            try
            {
                // Create a new brokered message to send to the queue
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToAdd)));
                await _queueClient.SendAsync(message);
            }
            catch(Exception EX)
            {
                string xxx = EX.Message;
                // TODO: Exception Handling here
            }
        }
    }
}
