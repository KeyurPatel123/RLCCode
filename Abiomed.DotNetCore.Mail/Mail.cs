using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;
using System.Threading.Tasks;
using Abiomed.Models;
using Newtonsoft.Json;

namespace Abiomed.DotNetCore
{
    public class Mail : IMail
    {
        private MailboxAddress _fromMailboxAddress;
        private string _textPart = string.Empty;
        private string _smtpClientName  = string.Empty;
        private string _smtpHostName = string.Empty;
        private string _localDomain = string.Empty;
        private int _port = 25;

        public Mail(string fromEmailAddress, string fromFriendlyName, string localDomain, string textPart, string smtpHostName, int port)
        {
            _fromMailboxAddress = new MailboxAddress(fromFriendlyName, fromEmailAddress);
            _localDomain = localDomain; 
            _textPart = textPart; 
            _smtpHostName = smtpHostName;
            _port = port; 
        }

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
    }
}
