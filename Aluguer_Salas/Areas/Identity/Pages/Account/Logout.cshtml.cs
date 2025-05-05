

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Aluguer_Salas.Data; // <<< IMPORTANTE: Using para encontrar Utilizadores
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

// Certifique-se que o namespace corresponde � estrutura das suas pastas
namespace Aluguer_Salas.Areas.Identity.Pages.Account
{
    // Permite que mesmo utilizadores n�o autenticados (ou j� deslogados) acedam a esta p�gina
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        // Injeta o SignInManager para a classe Utilizadores
        private readonly SignInManager<Utilizadores>
    _signInManager;
        private readonly ILogger<LogoutModel>
            _logger;

        public LogoutModel(SignInManager<Utilizadores>
            signInManager, ILogger<LogoutModel>
                logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        // OnGet normalmente n�o � usado para o logout direto via POST,
        // mas pode ser mantido se quiseres ter uma p�gina interm�dia (n�o comum).
        // Se o formul�rio em _LoginPartial faz POST direto, este OnGet pode nem ser chamado.
        public IActionResult OnGet()
        {
            // Poderia retornar uma view de confirma��o, mas normalmente o POST � direto.
            // Se o OnPost fizer o trabalho, podemos at� redirecionar daqui se algu�m aceder via GET por engano.
            // return RedirectToAction("Index", "Home"); // Ou apenas retornar a p�gina vazia
            return Page();
        }

        // Este m�todo � chamado quando o formul�rio de logout (com method="post") � submetido
        public async Task<IActionResult>
            OnPost(string? returnUrl = null)
        {
            // Efetua o logout do utilizador atual
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Utilizador deslogado com sucesso.");

            // Verifica se foi fornecido um URL de retorno
            if (returnUrl != null)
            {
                // Redireciona para o URL de retorno SE for um URL local (por seguran�a)
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Se n�o houver URL de retorno, redireciona para uma p�gina padr�o.
                // Normalmente redireciona para a p�gina inicial da aplica��o principal.
                // Certifique-se que o Page "/Index" existe ou use RedirectToAction.
                // return RedirectToPage("/Index"); // Pode precisar ajustar isto
                return RedirectToAction("Index", "Home"); // Mais comum para ir para a raiz da app
            }
        }
    }
}
