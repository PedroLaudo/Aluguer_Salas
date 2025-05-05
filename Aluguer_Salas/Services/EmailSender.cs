using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Threading.Tasks;

namespace Aluguer_Salas.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Aqui colocas a lógica de envio via SMTP, SendGrid, etc.
            Console.WriteLine($"[EmailSender] Para: {email}\nAssunto: {subject}\nMensagem: {htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
