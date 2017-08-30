using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System;
using System.Threading.Tasks;

namespace Abiomed.DotNetCore
{
    public class Mail
    {
        private MailboxAddress _fromMailbox;
        private string _textPart { get; set; } = string.Empty;

        private string _smtpClientName { get; set; } = string.Empty;
        private string _SmtpHostname { get; set; } = string.Empty;

        public Mail()
        {

        }

        public Mail(string configurationTableName)
        {

        }

        public async Task SendEmailAsync(string to, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Santa Claus", "santa_abiomed@outlook.com"));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = "www.abiomed.com"; // "some.domain.com";
                await client.ConnectAsync(@"USDVREX01.abiomed.com", 25, SecureSocketOptions.None).ConfigureAwait(false);
                //await client.ConnectAsync("smtp.relay.uri", 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }

}
