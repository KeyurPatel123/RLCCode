using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Threading;
using Abiomed.DotNetCore.Storage;
using Newtonsoft.Json;
using Abiomed.DotNetCore.Models;
using Abiomed.DotNetCore.Configuration;
using Abiomed.DotNetCore.Mail;
using Abiomed.DotNetCore.ServiceBus;

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
        private const string _auditLogManagerCannotBeNull = "Audit Log Manager cannot be null";
        private const string _configurationCacheCannotBeNull = "ConfigurationCache cannot be null";
        private const string _smtpManagerTypeNotConfigured = "SMTP Manager Type (Queue or Service Bus) is not defined";
        private const string _smtpActorNotConfigured = "SMTP Actor (Listener or Broadcaster) not defined";

        private EmailServiceActor _runningAs = new EmailServiceActor();

        static private IMail _mail;
        private IServiceBus _serviceBus;
        private IQueueClient _queueClient;
        private IAuditLogManager _auditLogManager;
        private IConfigurationCache _configurationCache;

        // Stop Gap until Service Bus Works
        private IQueueStorage _queueStorage;

        private bool _isServiceBusMode = false;

        #endregion

        #region Constructors

        public EmailManager(IAuditLogManager auditLogManager, IConfigurationCache configurationCache)
        {
            if (auditLogManager == null)
            {
                throw new ArgumentNullException(_auditLogManagerCannotBeNull);
            }

            if (configurationCache == null)
            {
                throw new ArgumentNullException(_configurationCacheCannotBeNull);
            }

            // Is it a queue or service bus
            if (!Enum.TryParse(configurationCache.GetConfigurationItem("smtpmanager", "emailservicetype"), out EmailServiceType emailServiceType))
            {
                throw new ArgumentOutOfRangeException(_smtpManagerTypeNotConfigured);
            }

            string queueName = configurationCache.GetConfigurationItem("smtpmanager", "queuename");
            ValidateRequiredString(queueName, "Queue Name");

            if (!Enum.TryParse(configurationCache.GetConfigurationItem("smtpmanager", "emailserviceactor"), out EmailServiceActor emailServiceActor))
            {
                throw new ArgumentOutOfRangeException(_smtpActorNotConfigured);
            }

            _auditLogManager = auditLogManager;
            _configurationCache = configurationCache;
            _runningAs = emailServiceActor;
            _mail = new Mail.Mail(_configurationCache);

            switch (emailServiceType)
            {
                case EmailServiceType.ServiceBus:
                    _isServiceBusMode = true;
                    _serviceBus = new ServiceBus.ServiceBus(configurationCache);
                    break;
                case EmailServiceType.Queue:
                default:
                    _queueStorage = new QueueStorage();
                    _queueStorage.SetQueueAsync(queueName);
                    _isServiceBusMode = false;
                    break;
            }
        }

        #endregion

        #region Public Methods

        #region Listeners

        public void Listen()
        {
            if (_runningAs != EmailServiceActor.Listener)
            {
                throw new InvalidOperationException(_invalidListenerOperation);
            }

            if (!_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInQueueMode);
            }

            _queueClient = _serviceBus.GetQueueClient();

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
                // TODO Better Logging of Message
                Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }
        }

        public async Task ListenToQueueStorage()
        {
            if (_isServiceBusMode)
            {
                throw new InvalidOperationException(_instanceIsInServiceBusMode);
            }
          
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
