using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Threading.Tasks;
using Abiomed.DotNetCore.Models;
using Newtonsoft.Json;
using Abiomed.DotNetCore.Configuration;

namespace Abiomed.DotNetCore.Mail
{
    public class Mail : IMail
    {
        #region Member Variables

        private MailboxAddress _fromMailboxAddress;
        private string _textPart = string.Empty;
        private string _smtpClientName = string.Empty;
        private string _smtpHostName = string.Empty;
        private string _localDomain = string.Empty;
        private int _port = 25;

        private bool _rerouteTests = false;
        private string _rerouteEmail = string.Empty;

        #endregion

        #region Constructors

        public Mail(IConfigurationCache configurationCache)
        {
            string fromFriendlyName = configurationCache.GetConfigurationItem("smtpmanager", "fromfriendlyname");
            string fromEmailAddress = configurationCache.GetConfigurationItem("smtpmanager", "fromemail");
            _fromMailboxAddress = new MailboxAddress(fromFriendlyName, fromEmailAddress);
            _localDomain = configurationCache.GetConfigurationItem("smtpmanager", "localdomain");
            _textPart = configurationCache.GetConfigurationItem("smtpmanager", "bodytexttype");
            _smtpHostName = configurationCache.GetConfigurationItem("smtpmanager", "host");
            _port = configurationCache.GetNumericConfigurationItem("smtpmanager", "portnumber");

            _rerouteTests = configurationCache.GetBooleanConfigurationItem("smtpmanager", "rerouteexternaltointernal");
            if (_rerouteTests)
            {
                _rerouteEmail = configurationCache.GetConfigurationItem("smtpmanager", "rerouteexternalemailsto");
            }

        }

        #endregion

        #region Public Methods

        public async Task SendEmailAsync(string jsonMessage)
        {
            Email email = JsonConvert.DeserializeObject<Email>(jsonMessage);
            await SendEmailAsync(email.To, email.Subject, email.Body, email.ToFriendlyName, email.From, email.FromFriendlyName);
        }

        public async Task SendEmailAsync(Email email)
        {
            await SendEmailAsync(email.To, email.Subject, email.Body, email.ToFriendlyName, email.From, email.FromFriendlyName);
        }

        public async Task SendEmailAsync(string to, string subject, string body, string toFriendlyName, string fromEmail, string fromFriendlyName)
        {
            var emailMessage = new MimeMessage();

            MailboxAddress fromMailBoxAddress = _fromMailboxAddress;
            if (!string.IsNullOrWhiteSpace(fromEmail))
            {
                fromMailBoxAddress = new MailboxAddress(fromFriendlyName, fromEmail);
            }

            if (_rerouteTests)
            {
                string domain = to.Substring(to.IndexOf('@'), 7);
                if (domain.ToLower() != "abiomed")
                {
                    to = _rerouteEmail;
                }
            }

            MailboxAddress toMailBoxAddress = new MailboxAddress(toFriendlyName, to);
            emailMessage.From.Add(fromMailBoxAddress);
            emailMessage.To.Add(toMailBoxAddress);
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(_textPart) { Text = body };


            using (var client = new SmtpClient())
            {
                client.LocalDomain = _localDomain;
                await client.ConnectAsync(_smtpHostName, _port, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

        #endregion
    }
}