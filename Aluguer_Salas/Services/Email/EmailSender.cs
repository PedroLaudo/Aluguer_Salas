using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions; // Para extrair o link, se necessário

namespace Aluguer_Salas.Services.Email
{
    public class EmailSender : ICustomEmailSender, IEmailSender // Mantém ambas as interfaces se ICustomEmailSender for usada noutro lado
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // Método para ICustomEmailSender (ou seu método principal se não for chamado pelo Identity)
        // Pode renomeá-lo para evitar confusão, ex: SendCustomFormattedEmailAsync
        public async Task SendCustomConfirmationEmailAsync(string email, string name, string confirmationLink)
        {
            _logger.LogInformation($"SendCustomConfirmationEmailAsync: Preparing to send email to {email} for {name} with link {confirmationLink}");
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                // Para o campo 'To', MailboxAddress espera (opcionalmente) um nome e um endereço.
                // Se 'name' for o nome do destinatário, está correto.
                message.To.Add(new MailboxAddress(name, email));
                message.Subject = "Confirme o seu registo - Aluguer de Salas"; // Assunto mais específico

                var builder = new BodyBuilder();
                builder.HtmlBody = GetConfirmationEmailHtml(name, confirmationLink);
                builder.TextBody = GetConfirmationEmailText(name, confirmationLink);
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                // Adicionar log de protocolo para depuração SMTP se necessário:
                // client.ProtocolLogger = new ProtocolLogger(Console.OpenStandardOutput());

                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTlsWhenAvailable); // StartTlsWhenAvailable é mais flexível
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email personalizado de confirmação enviado para {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Falha ao enviar email personalizado de confirmação para {email}");
                throw;
            }
        }

        // *** ESTA É A IMPLEMENTAÇÃO QUE O ASP.NET CORE IDENTITY VAI CHAMAR ***
        public async Task SendEmailAsync(string email, string subject, string htmlMessageFromIdentity)
        {
            _logger.LogInformation($"IEmailSender.SendEmailAsync: Email to {email}, Subject: {subject}");
            _logger.LogDebug($"IEmailSender.SendEmailAsync: Raw htmlMessageFromIdentity: {htmlMessageFromIdentity}");

            // O 'htmlMessageFromIdentity' passado pelo ASP.NET Core Identity
            // para confirmação de email é geralmente o link de callback ou um HTML simples com o link.
            // Precisamos do link e de um nome (que não é fornecido por esta interface).

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
                // Se não for um link HTML, assumir que htmlMessageFromIdentity é o próprio link (menos comum para Identity)
                // ou tratar como erro se esperado um formato específico.
                // Por segurança, usar o htmlMessageFromIdentity como link se não encontrar um padrão <a>.
                confirmationLink = htmlMessageFromIdentity;
                _logger.LogWarning($"Não foi possível extrair o link de <a> em htmlMessageFromIdentity. Usando a mensagem inteira como link: {confirmationLink}");
            }

            // A interface IEmailSender não fornece o 'nome' do utilizador.
            // Soluções:
            // 1. Usar um nome genérico.
            // 2. Tentar extrair o nome do endereço de email (parte antes do @).
            // 3. (Mais complexo) Se precisar MESMO do nome real, teria que o obter da base de dados aqui,
            //    o que tornaria o EmailSender dependente do UserManager ou DbContext.
            string userName = email.Contains("@") ? email.Split('@')[0] : "Utilizador"; // Opção 2 (simples)

            _logger.LogInformation($"IEmailSender.SendEmailAsync: Utilizador inferido: {userName}, Link de confirmação extraído/usado: {confirmationLink}");

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                message.To.Add(MailboxAddress.Parse(email)); // Identity só fornece o email para o 'To'
                message.Subject = subject; // Usar o assunto fornecido pelo Identity

                var builder = new BodyBuilder();
                // Usar o seu template HTML personalizado
                builder.HtmlBody = GetConfirmationEmailHtml(userName, confirmationLink);
                builder.TextBody = GetConfirmationEmailText(userName, confirmationLink);
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                // client.ProtocolLogger = new ProtocolLogger(Console.OpenStandardOutput()); // Para depuração
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

        // Seus métodos de geração de HTML e texto permanecem os mesmos
        private string GetConfirmationEmailHtml(string name, string link)
        {
            // Adicionar DOCTYPE e estilos básicos para melhor compatibilidade
            return $@"
            <!DOCTYPE html>
            <html lang='pt'>
            <head>
                <meta charset='UTF-8'>
                <title>Confirmação de Email</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; color: #333; }}
                    .button {{
                        background-color: #007bff; /* Cor de fundo do botão */
                        color: #ffffff; /* Cor do texto do botão (BRANCO) */
                        padding: 10px 15px;   /* Espaçamento interno */
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
                <p>Se não solicitou este email, por favor, ignore-o.</p>
            </body>
            </html>";
        }

        private string GetConfirmationEmailText(string name, string link)
        {
            return $@"
            Bem-vindo ao Aluguer de Salas, {name}!

            Por favor, confirme o seu email visitando este link:
            {link}

            Se não solicitou este email, por favor, ignore-o.";
        }
    }
}