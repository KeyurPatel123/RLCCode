using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore.Storage
{
    public class QueueStorage : IQueueStorage
    {
        #region Member Variables

        private const string QueueNameCannotBeNull = @"Queue Name cannot be null, empty, or whitespace.";
        private const string QueueMessageCannotBeNull = @"Queue Message cannot be null";
        private const string NumberOfMessagesToRetrieve = @"Number of messages to retrieve must be > 0";
        private const string connectionStringCannotbeNull = @"Connection String cannot be null, empty, or whitespace.";

        private CloudQueueClient _queueClient = null;
        private CloudQueue _queue = null;
        private CloudStorageAccount _storageAccount = null;
        private CloudQueueMessage _retrievedMessage = null;

        #endregion

        #region Constructors

        public QueueStorage(string connection, string queueName = "")
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new ArgumentOutOfRangeException(QueueNameCannotBeNull);
            }

            Initialize(connection);

            if (!string.IsNullOrEmpty(queueName))
            {
                // Create the Table.
                SetQueueAsync(queueName).Wait();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a Message to the Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToAdd"></param>
        /// <returns></returns>
        public async Task AddMessageAsync<T>(T objectToAdd)
        {
            if (objectToAdd == null)
            {
                throw new ArgumentNullException(QueueMessageCannotBeNull);
            }

            var messageAsJson = JsonConvert.SerializeObject(objectToAdd);
            await _queue.AddMessageAsync(new CloudQueueMessage(messageAsJson));
        }

        /// <summary>
        /// Peeks the Next message in the queue.
        /// </summary>
        /// <returns>String (JSON) Message</returns>
        public async Task<string> PeekMessageAsync() 
        {
            string result = string.Empty;
            var peekedMessage = await _queue.PeekMessageAsync();

            if (peekedMessage != null)
            {
                result = peekedMessage.AsString;
            }

            return result;
        }

        /// <summary>
        /// This will peek the messages int he queue and will retrieve upto the next number of messages.
        /// An empty list is returned if there are no messages int he queue
        /// </summary>
        /// <param name="numberOfMessagesToPeek"></param>
        /// <returns>List of string (JSON) messages</returns>
        public async Task<List<string>> PeekMessagesAsync(int numberOfMessagesToPeek)
        {
            if (numberOfMessagesToPeek < 1)
            {
                throw new ArgumentOutOfRangeException(NumberOfMessagesToRetrieve);
            }

            var peekedMessages = await _queue.PeekMessagesAsync(numberOfMessagesToPeek);

            List<String> results = new List<string>();
            foreach(var peekedMessage in peekedMessages)
            {
                results.Add(peekedMessage.AsString);
            }

            return results;
        }

        /// <summary>
        /// Retreieves the Next Message from the queue for processing.
        /// This message is removed from view and must be processed and then deleted within 30-seconds
        /// or it will be put back in the queue and the delete will fail.
        /// </summary>
        /// <returns></returns>
        public async Task RetrieveMessageAsync()
        {
           _retrievedMessage = await _queue.GetMessageAsync();
        }

        /// <summary>
        /// Deletes the retrieved message
        /// </summary>
        /// <returns></returns>
        public async Task DeleteRetrievedMessageAsync()
        {
            try
            {
                await _queue.DeleteMessageAsync(_retrievedMessage);
            } catch 
            {
                // There are no messages to Delete
                // TODO Handle Exception - Log it.
            }

            _retrievedMessage = null;
        }

        /// <summary>
        /// Returns the Message Content of a Pulled message
        /// </summary>
        /// <returns>String (JSON) of the retrieved message</returns>
        public string GetMessageContent()
        {
            if (_retrievedMessage != null)
            {
                return _retrievedMessage.AsString;
            }
            return string.Empty;
        }

        /// <summary>
        /// Changes the Queue to reference a New Queue...
        /// </summary>
        /// <param name="newQueueName">The name of the new queue</param>
        public async Task ChangeQueueAsync(string newQueueName)
        {
            if (string.IsNullOrWhiteSpace(newQueueName))
            {
                throw new ArgumentOutOfRangeException(QueueNameCannotBeNull);
            }

            await SetQueueAsync(newQueueName);
        }

        /// <summary>
        /// Returns The Queue Name of the curtrent Queue being used
        /// </summary>
        /// <returns>The Queue Name</returns>
        public string GetMyQueueName()
        {
            return _queue.Name;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shared Constructor Initialization Code
        /// </summary>
        private void Initialize(string storageConnection)
        {
            _storageAccount = CloudStorageAccount.Parse(storageConnection);
            _queueClient = _storageAccount.CreateCloudQueueClient();
        }

        /// <summary>
        /// Sets the Context as to which Queue is the active Queue being used by this instance
        /// </summary>
        /// <param name="queueName"></param>
        private async Task SetQueueAsync(string queueName)
        {
            _queue = _queueClient.GetQueueReference(queueName);
            await _queue.CreateIfNotExistsAsync();
        }

        #endregion
    }
}