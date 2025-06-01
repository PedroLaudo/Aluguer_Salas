using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web; // Para HtmlEncoder
using System.Text; // Para Encoding
using Microsoft.AspNetCore.WebUtilities; // Para WebEncoders
using Microsoft.AspNetCore.Authorization; // � uma boa pr�tica adicionar para p�ginas de conta

// Certifique-se de que tem o using para a sua classe Utilizadores
// Se Utilizadores estiver em Aluguer_Salas.Data:
using Aluguer_Salas.Data;

// Se a sua p�gina estiver numa �rea Identity (comum com scaffolding):
// namespace SeuProjeto.Areas.Identity.Pages.Account
// {
[AllowAnonymous] // Permite acesso an�nimo a esta p�gina
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
        [Required(ErrorMessage = "O campo Email � obrigat�rio.")]
        [EmailAddress(ErrorMessage = "O Email n�o � um endere�o de email v�lido.")]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) // Se o modelo n�o for v�lido, retorna a p�gina com os erros.
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // N�o revelar que o usu�rio n�o existe ou n�o est� confirmado para evitar enumera��o de usu�rios
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // Gera o token de reset de senha
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        // � uma boa pr�tica codificar o token para uso em URLs
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword", // Certifique-se que este � o caminho correto para sua p�gina ResetPassword
            pageHandler: null,
            // Removi 'email' daqui pois o 'code' geralmente � suficiente e mais seguro.
            // Se sua p�gina ResetPassword precisar do email explicitamente na URL, voc� pode adicion�-lo.
            // Mas o token j� est� associado ao usu�rio.
            values: new { area = "Identity", code = code }, // Se estiver usando �reas Identity
            protocol: Request.Scheme);

        await _emailSender.SendEmailAsync(
            Input.Email,
            "Redefinir senha",
            $"Por favor, redefina sua senha <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}