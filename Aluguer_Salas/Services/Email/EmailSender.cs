using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Aluguer_Salas.Services.Email
{
    public class EmailSender : ICustomEmailSender, IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string name , string confirmationLink)
        {
            Console.WriteLine("SendRegistrationConfirmationAsync called");
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                message.To.Add(new MailboxAddress(name, email));
                message.Subject = "Confirm Your Connectify Registration";

                var builder = new BodyBuilder();

                // HTML version
                builder.HtmlBody = GetConfirmationEmailHtml(name, confirmationLink);

                // Plain text version
                builder.TextBody = GetConfirmationEmailText(name, confirmationLink);

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Confirmation email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send confirmation email to {email}");
                throw; // Re-throw for calling code to handle
            }
        }

        private string GetConfirmationEmailHtml(string name, string link)
        {
            return $@"
            <html>
                <body>
                    <h1>Welcome to Aluguer de Salas, {name}!</h1>
                    <p>Please confirm your email:</p>
                    <p><a href='{link}'>Confirm Email</a></p>
                    <p>If you didn't request this, please ignore this email.</p>
                </body>
            </html>";
        }



        private string GetConfirmationEmailText(string name, string link)
        {
            return $@"
            Welcome to Aluguer de Salas, {name}!

            Please confirm your email by visiting this link:
            {link}

            If you didn't request this, please ignore this email.";
        }
    }
}
