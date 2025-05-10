#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Aluguer_Salas.Data; // Namespace para Utilizadores e Utentes e ApplicationDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem
using Microsoft.EntityFrameworkCore;

namespace Aluguer_Salas.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IUserStore<Utilizador> _userStore;
        private readonly IUserEmailStore<Utilizador> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<Utilizador> userManager,
            IUserStore<Utilizador> userStore,
            SignInManager<Utilizador> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public List<SelectListItem> TiposDeUtilizador { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Selecione o Tipo --" },
            new SelectListItem { Value = "Aluno", Text = "Aluno" },
            new SelectListItem { Value = "Professor", Text = "Professor" }
        };

        public class InputModel
        {
            [Required(ErrorMessage = "O campo Nome é obrigatório.")]
            [Display(Name = "Nome completo")]
            public string Nome { get; set; }

            [Required(ErrorMessage = "O campo Email é obrigatório.")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "O campo Password é obrigatório.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Password")]
            [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Por favor, selecione o tipo de utilizador.")]
            [Display(Name = "Tipo de Utilizador")]
            public string Tipo { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Nome = Input.Nome;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User (Identity) created a new account with password.");

                    var utente = new Utente
                    {
                        UtilizadorIdentityId = user.Id,
                        Email = user.Email,
                        Tipo = Input.Tipo
                    };

                    _context.Utentes.Add(utente);
                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Associated Utente record created for user {UserId}.", user.Id);
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "Error saving Utente record for user {UserId}.", user.Id);
                        ModelState.AddModelError(string.Empty, "Ocorreu um erro ao guardar os dados do perfil do utilizador.");
                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    try
                    {
                        await _emailSender.SendEmailAsync(Input.Email, "Confirme o seu email",
                            $"Por favor, confirme a sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");
                        _logger.LogInformation("Confirmation email sent (or logged) for {Email}", Input.Email);
                    }
                    catch (Exception ex_email)
                    {
                        _logger.LogError(ex_email, "Error sending confirmation email for {Email}", Input.Email);
                        // Mesmo que o envio do email falhe, queremos que o usuário seja informado
                        // sobre a necessidade de confirmação, então prosseguimos para RegisterConfirmation.
                    }


                    // --- INÍCIO DA MODIFICAÇÃO ---
                    // Logar o valor da opção para diagnóstico
                    bool requiresAccountConfirmation = _userManager.Options.SignIn.RequireConfirmedAccount;
                    _logger.LogInformation("DEBUG: UserManager.Options.SignIn.RequireConfirmedAccount is: {RequiresConfirmation}", requiresAccountConfirmation);

                    // Forçar o redirecionamento para a página RegisterConfirmation
                    _logger.LogInformation("Forcing redirect to RegisterConfirmation page for user {Email}.", Input.Email);
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });

                    // CÓDIGO ORIGINAL (agora comentado para forçar o redirecionamento):
                    /*
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                    */
                    // --- FIM DA MODIFICAÇÃO ---
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private Utilizador CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Utilizador>();
            }
            catch
            {
                throw new InvalidOperationException($"Não foi possível criar uma instância de '{nameof(Utilizador)}'. " +
                    $"Assegura-te que '{nameof(Utilizador)}' não é abstrata e tem um construtor sem parâmetros, ou, alternativamente, " +
                    "sobrescreve a página de registo em /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<Utilizador> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("O UI padrão requer um user store que suporte email.");
            }
            return (IUserEmailStore<Utilizador>)_userStore;
        }
    }
}