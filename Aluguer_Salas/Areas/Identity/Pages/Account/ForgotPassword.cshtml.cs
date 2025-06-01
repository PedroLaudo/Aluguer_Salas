using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web; // Para HtmlEncoder
using System.Text; // Para Encoding
using Microsoft.AspNetCore.WebUtilities; // Para WebEncoders
using Microsoft.AspNetCore.Authorization; // É uma boa prática adicionar para páginas de conta

// Certifique-se de que tem o using para a sua classe Utilizadores
// Se Utilizadores estiver em Aluguer_Salas.Data:
using Aluguer_Salas.Data;

// Se a sua página estiver numa área Identity (comum com scaffolding):
// namespace SeuProjeto.Areas.Identity.Pages.Account
// {
[AllowAnonymous] // Permite acesso anônimo a esta página
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<Utilizadores> _userManager; // <<< ALTERADO AQUI
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<Utilizadores> userManager, IEmailSender emailSender) // <<< ALTERADO AQUI
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O Email não é um endereço de email válido.")]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) // Se o modelo não for válido, retorna a página com os erros.
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Não revelar que o usuário não existe ou não está confirmado para evitar enumeração de usuários
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // Gera o token de reset de senha
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        // É uma boa prática codificar o token para uso em URLs
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword", // Certifique-se que este é o caminho correto para sua página ResetPassword
            pageHandler: null,
            // Removi 'email' daqui pois o 'code' geralmente é suficiente e mais seguro.
            // Se sua página ResetPassword precisar do email explicitamente na URL, você pode adicioná-lo.
            // Mas o token já está associado ao usuário.
            values: new { area = "Identity", code = code }, // Se estiver usando áreas Identity
            protocol: Request.Scheme);

        await _emailSender.SendEmailAsync(
            Input.Email,
            "Redefinir senha",
            $"Por favor, redefina sua senha <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}