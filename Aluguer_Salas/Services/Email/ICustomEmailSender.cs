namespace Aluguer_Salas.Services.Email
{
    public interface ICustomEmailSender
    {
        Task SendEmailAsync(string email, string name, string confirmationLink);
    }
}
