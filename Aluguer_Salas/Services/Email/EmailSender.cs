using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions; // Para extrair o link, se necess�rio

namespace Aluguer_Salas.Services.Email
{
    public class EmailSender : ICustomEmailSender, IEmailSender // Mant�m ambas as interfaces se ICustomEmailSender for usada noutro lado
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // M�todo para ICustomEmailSender (ou seu m�todo principal se n�o for chamado pelo Identity)
        // Pode renome�-lo para evitar confus�o, ex: SendCustomFormattedEmailAsync
        public async Task SendCustomConfirmationEmailAsync(string email, string name, string confirmationLink)
        {
            _logger.LogInformation($"SendCustomConfirmationEmailAsync: Preparing to send email to {email} for {name} with link {confirmationLink}");
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                // Para o campo 'To', MailboxAddress espera (opcionalmente) um nome e um endere�o.
                // Se 'name' for o nome do destinat�rio, est� correto.
                message.To.Add(new MailboxAddress(name, email));
                message.Subject = "Confirme o seu registo - Aluguer de Salas"; // Assunto mais espec�fico

                var builder = new BodyBuilder();
                builder.HtmlBody = GetConfirmationEmailHtml(name, confirmationLink);
                builder.TextBody = GetConfirmationEmailText(name, confirmationLink);
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                // Adicionar log de protocolo para depura��o SMTP se necess�rio:
                // client.ProtocolLogger = new ProtocolLogger(Console.OpenStandardOutput());

                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTlsWhenAvailable); // StartTlsWhenAvailable � mais flex�vel
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email personalizado de confirma��o enviado para {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Falha ao enviar email personalizado de confirma��o para {email}");
                throw;
            }
        }

        // *** ESTA � A IMPLEMENTA��O QUE O ASP.NET CORE IDENTITY VAI CHAMAR ***
        public async Task SendEmailAsync(string email, string subject, string htmlMessageFromIdentity)
        {
            _logger.LogInformation($"IEmailSender.SendEmailAsync: Email to {email}, Subject: {subject}");
            _logger.LogDebug($"IEmailSender.SendEmailAsync: Raw htmlMessageFromIdentity: {htmlMessageFromIdentity}");

            // O 'htmlMessageFromIdentity' passado pelo ASP.NET Core Identity
            // para confirma��o de email � geralmente o link de callback ou um HTML simples com o link.
            // Precisamos do link e de um nome (que n�o � fornecido por esta interface).

            string confirmationLink;
            // Tentar extrair o link se htmlMessageFromIdentity for um HTML simples
            // Exemplo: <a href='LINK'>
            var match = Regex.Match(htmlMessageFromIdentity, @"<a\s+(?:[^>]*?\s+)?href=([""'])(.*?)\1");
            if (match.Success)
            {
                confirmationLink = match.Groups[2].Value;
            }
            else
            {
                // Se n�o for um link HTML, assumir que htmlMessageFromIdentity � o pr�prio link (menos comum para Identity)
                // ou tratar como erro se esperado um formato espec�fico.
                // Por seguran�a, usar o htmlMessageFromIdentity como link se n�o encontrar um padr�o <a>.
                confirmationLink = htmlMessageFromIdentity;
                _logger.LogWarning($"N�o foi poss�vel extrair o link de <a> em htmlMessageFromIdentity. Usando a mensagem inteira como link: {confirmationLink}");
            }

            // A interface IEmailSender n�o fornece o 'nome' do utilizador.
            // Solu��es:
            // 1. Usar um nome gen�rico.
            // 2. Tentar extrair o nome do endere�o de email (parte antes do @).
            // 3. (Mais complexo) Se precisar MESMO do nome real, teria que o obter da base de dados aqui,
            //    o que tornaria o EmailSender dependente do UserManager ou DbContext.
            string userName = email.Contains("@") ? email.Split('@')[0] : "Utilizador"; // Op��o 2 (simples)

            _logger.LogInformation($"IEmailSender.SendEmailAsync: Utilizador inferido: {userName}, Link de confirma��o extra�do/usado: {confirmationLink}");

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                message.To.Add(MailboxAddress.Parse(email)); // Identity s� fornece o email para o 'To'
                message.Subject = subject; // Usar o assunto fornecido pelo Identity

                var builder = new BodyBuilder();
                // Usar o seu template HTML personalizado
                builder.HtmlBody = GetConfirmationEmailHtml(userName, confirmationLink);
                builder.TextBody = GetConfirmationEmailText(userName, confirmationLink);
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                // client.ProtocolLogger = new ProtocolLogger(Console.OpenStandardOutput()); // Para depura��o
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTlsWhenAvailable);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email (via IEmailSender) enviado para {email} com o assunto '{subject}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Falha ao enviar email (via IEmailSender) para {email} com o assunto '{subject}'.");
                throw;
            }
        }

        // Seus m�todos de gera��o de HTML e texto permanecem os mesmos
        private string GetConfirmationEmailHtml(string name, string link)
        {
            // Adicionar DOCTYPE e estilos b�sicos para melhor compatibilidade
            return $@"
            <!DOCTYPE html>
            <html lang='pt'>
            <head>
                <meta charset='UTF-8'>
                <title>Confirma��o de Email</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; color: #333; }}
                    .button {{
                        background-color: #007bff; /* Cor de fundo do bot�o */
                        color: #ffffff; /* Cor do texto do bot�o (BRANCO) */
                        padding: 10px 15px;   /* Espa�amento interno */
                        text-decoration: none; /* Remove o sublinhado do link */
                        border-radius: 5px;    /* Cantos arredondados */
                        display: inline-block; /* Para melhor comportamento do padding e margens */
                        font-weight: bold;     /* Opcional: texto a negrito */
                    }}
                    .button:hover {{
                        background-color: #0056b3; /* Opcional: cor ao passar o mouse */
                    }}
                </style>
            </head>
            <body>
                <h1>Bem-vindo ao Aluguer de Salas, {name}!</h1>
                <p>Por favor, confirme o seu email clicando no link abaixo:</p>
                <p>
                    <a href='{link}' class='button'>Confirmar Email</a>
                </p>
                <p>Se n�o solicitou este email, por favor, ignore-o.</p>
            </body>
            </html>";
        }

        private string GetConfirmationEmailText(string name, string link)
        {
            return $@"
            Bem-vindo ao Aluguer de Salas, {name}!

            Por favor, confirme o seu email visitando este link:
            {link}

            Se n�o solicitou este email, por favor, ignore-o.";
        }
    }
}