using System;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Abiomed.DotNetCore.Configuration;

namespace Abiomed.DotNetCore.ServiceBus
{

    public class ServiceBus : IServiceBus
    {
        #region Member Variables

        private const string _messageCannotBeNull = "Message cannot be null.";
        private const string _queueNameCannotBeEmpty = "Queue Name cannot be null or empty.";
        private const string _connectionStringCannotBeEmpty = "Connection String cannot be null or empty";
        private const string _configurationCacheCannotBeNull = "Configuration Cache cannot be null";

        private IConfigurationCache _configurationCache;
        private IQueueClient _queueClient;

        #endregion

        #region Constructors

        public ServiceBus(IConfigurationCache configurationCache)
        {
            if (configurationCache == null)
            {
                throw new ArgumentNullException(_configurationCacheCannotBeNull);
            }

            string queueName = configurationCache.GetConfigurationItem("smtpmanager", "queuename");

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentOutOfRangeException(_queueNameCannotBeEmpty);
            }

            string connection = configurationCache.GetConfigurationItem("smtpmanager", "servicebusconnection");
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentOutOfRangeException(_connectionStringCannotBeEmpty);
            }

            _queueClient = new QueueClient(connection, queueName, ReceiveMode.PeekLock);
        }

        #endregion

        #region Public Methods
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

        #endregion
    }
}
