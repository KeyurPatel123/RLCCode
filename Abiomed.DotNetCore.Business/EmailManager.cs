using System;
using System.Collections.Generic;
using System.Text;
using Abiomed.DotNetCore.Communication;
using Abiomed.Models;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Threading;
using Abiomed.DotNetCore.Storage;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore.Business
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailManager : IEmailManager
    {
        #region Member Variables
        private const string _invalidListenerOperation = "Listener cannot add to service bus.";
        private const string _invalidBroadcasterOperation = "Broadcaster cannot read from service bus.";
        private const string _serviceActorMustBeListenerOrBroadcaster = "Service Actor must be either broadcaster or listener";
        private const string _instanceIsInQueueMode = "Running in Queue Storage Mode";
        private const string _instanceIsInServiceBusMode = "Running in Service Bus Mode";
        private const string _portNumberMustBeGreaterThanZero = "Port number must be greater than zero";
        private const string _cannotBeNullEmptyOrWhitespace = " cannot be null, empty or whitespace";
        private const string _auditLogManagerCannotBeNull = "Audit Log Manager object cannot be null";

        private string _queueName = string.Empty;
        private string _connection = string.Empty;
        private EmailServiceActor _runningAs = new EmailServiceActor();

        static private IMail _mail;
        private IServiceBus _serviceBus;
        private IQueueClient _queueClient;
        private IAuditLogManager _auditLogManager;

        // Stop Gap until Service Bus Works
        private IQueueStorage _queueStorage;
        private bool _isServiceBusMode = false;

        #endregion

        #region Constructors
        public EmailManager(AuditLogManager auditLogManager, string queueStorageConnectionString, string queueName)
        {
            if (auditLogManager == null)
            {
                throw new ArgumentNullException(_auditLogManagerCannotBeNull);
            }
            ValidateRequiredString(queueName, "Queue Name");
            ValidateRequiredString(queueStorageConnectionString, "Storage Connection");

            _isServiceBusMode = false;
            _queueStorage = new QueueStorage(queueStorageConnectionString, queueName);
            _queueName = queueName;
            _auditLogManager = auditLogManager;
        }

        public EmailManager(AuditLogManager auditLogManager, string queueName, string connection, EmailServiceActor serviceActor)
        {
            if (auditLogManager == null)
            {
                throw new ArgumentNullException(_auditLogManagerCannotBeNull);
            }
            ValidateRequiredString(queueName, "Queue Name");
            ValidateRequiredString(connection, "Connection");

            if (serviceActor == EmailServiceActor.Unknown)
            {
                throw new ArgumentOutOfRangeException(_serviceActorMustBeListenerOrBroadcaster);
            }

            _isServiceBusMode = true;
            _serviceBus = new ServiceBus(queueName, connection);
            _runningAs = serviceActor;
            _auditLogManager = auditLogManager;
        }

        #endregion

        #region Public Methods

        #region Listeners
        public void Listen(string fromEmail,
                            string fromFriendlyName,
                            string localDomainName,
                            string smtpHostName,
                            int smtpPortNumber,
                            string textPart = "plain")
        {
            if (_runningAs != EmailServiceActor.Listener)
            {
                throw new InvalidOperationException(_invalidListenerOperation);
            }

            if (!_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInQueueMode);
            }
            ValidateRequiredString(fromEmail, "From Email");
            ValidateRequiredString(fromFriendlyName, "From Name");
            ValidateRequiredString(localDomainName, "Domain Name");
            ValidateRequiredString(smtpHostName, "Host Name");

            if (smtpPortNumber < 1)
            {
                throw new ArgumentOutOfRangeException(_portNumberMustBeGreaterThanZero);
            }

            _queueClient = _serviceBus.GetQueueClient();
            _mail = new Mail(fromEmail, fromFriendlyName, localDomainName, textPart, smtpHostName, smtpPortNumber);

            try
            {
                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                    // Set it according to how many messages the application wants to process in parallel.
                    MaxConcurrentCalls = 3,

                    // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                    // False value below indicates the Complete will be handled by the User Callback as seen in `ProcessMessagesAsync`.
                    AutoComplete = false
                };

                _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }
        }

        public async Task ListenToQueueStorage(string fromEmail,
                            string fromFriendlyName,
                            string localDomainName,
                            string smtpHostName,
                            int smtpPortNumber,
                            string textPart = "plain")
        {
            if (_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInServiceBusMode);
            }
            ValidateRequiredString(fromEmail, "From Email");
            ValidateRequiredString(fromFriendlyName, "From Name");
            ValidateRequiredString(localDomainName, "Domain Name");
            ValidateRequiredString(smtpHostName, "Host Name");
            if (smtpPortNumber < 1)
            {
                throw new ArgumentOutOfRangeException(_portNumberMustBeGreaterThanZero);
            }

            _mail = new Mail(fromEmail, fromFriendlyName, localDomainName, textPart, smtpHostName, smtpPortNumber);
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
                    Email email = JsonConvert.DeserializeObject<Email>(emailContent);
                    await _mail.SendEmailAsync(emailContent);
                    await _queueStorage.DeleteRetrievedMessageAsync();
                    processMessages = true;

                    await _auditLogManager.AuditAsync(email.To, DateTime.UtcNow, "", "Email Sent from Message Queue", email.Subject);
                }
            }
        }
        #endregion

        #region Broadcasters 
        public async Task BroadcastToQueueStorage(string to, string subject, string body, string toFriendlyName = "", string from = "", string fromFriendlyName = "")
        {
            if (_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInQueueMode);
            }
            ValidateRequiredString(to, "To");
            ValidateRequiredString(subject, "Subject");
            ValidateRequiredString(body, "Body");

            Email email = new Email();
            email.To = to;
            email.ToFriendlyName = toFriendlyName;
            email.Subject = subject;
            email.Body = body;
            email.From = from;
            email.FromFriendlyName = fromFriendlyName;

            await _queueStorage.AddMessageAsync(email);
            await _auditLogManager.AuditAsync(to, DateTime.UtcNow, "", "Email queued through message queue", subject);
        }

        public async Task Broadcast(string to, string subject, string body, string toFriendlyName = "", string from = "", string fromFriendlyName = "")
        {
            if (_runningAs != EmailServiceActor.Broadcaster)
            {
                throw new InvalidOperationException(_invalidBroadcasterOperation);
            }
            if (!_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInQueueMode);
            }

            ValidateRequiredString(to, "To");
            ValidateRequiredString(subject, "Subject");
            ValidateRequiredString(body, "Body");

            var email = new Email();
            email.To = to;
            email.Subject = subject;
            email.Body = body;
            email.From = from;
            email.FromFriendlyName = fromFriendlyName;
            email.ToFriendlyName = toFriendlyName;

            await _serviceBus.SendMessageAsync(email);
            await _auditLogManager.AuditAsync(to, DateTime.UtcNow, "", "Email queued through Service Bus", subject);
        }

        #endregion

        #region misc
        public async Task Stop()
        {
            if (!_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInQueueMode);
            }

            await _serviceBus.CloseAsync();
        }
        #endregion

        #endregion

        #region Private Members

        /// <summary>
        /// Handler ro process the exception when listening, Service Bus
        /// </summary>
        /// <param name="exceptionReceivedEventArgs"></param>
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            //TODO: Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Message Handler for Service Bus Listener
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            await _mail.SendEmailAsync(Encoding.UTF8.GetString(message.Body));
            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is opened in ReceiveMode.PeekLock mode.
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

            Email email = JsonConvert.DeserializeObject<Email>(Encoding.UTF8.GetString(message.Body));
            await _auditLogManager.AuditAsync(email.To, DateTime.UtcNow, "", "Email Sent from Service Bus", email.Subject);
        }

        private void ValidateRequiredString(string field, string friendlyFieldName)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ArgumentOutOfRangeException(friendlyFieldName + _cannotBeNullEmptyOrWhitespace);
            }
        }
        #endregion
    }
}
