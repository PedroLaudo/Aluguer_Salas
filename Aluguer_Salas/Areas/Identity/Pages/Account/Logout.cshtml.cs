

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Aluguer_Salas.Data; // <<< IMPORTANTE: Using para encontrar Utilizadores
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

// Certifique-se que o namespace corresponde à estrutura das suas pastas
namespace Aluguer_Salas.Areas.Identity.Pages.Account
{
    // Permite que mesmo utilizadores não autenticados (ou já deslogados) acedam a esta página
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

        // OnGet normalmente não é usado para o logout direto via POST,
        // mas pode ser mantido se quiseres ter uma página intermédia (não comum).
        // Se o formulário em _LoginPartial faz POST direto, este OnGet pode nem ser chamado.
        public IActionResult OnGet()
        {
            // Poderia retornar uma view de confirmação, mas normalmente o POST é direto.
            // Se o OnPost fizer o trabalho, podemos até redirecionar daqui se alguém aceder via GET por engano.
            // return RedirectToAction("Index", "Home"); // Ou apenas retornar a página vazia
            return Page();
        }

        // Este método é chamado quando o formulário de logout (com method="post") é submetido
        public async Task<IActionResult>
            OnPost(string? returnUrl = null)
        {
            // Efetua o logout do utilizador atual
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Utilizador deslogado com sucesso.");

            // Verifica se foi fornecido um URL de retorno
            if (returnUrl != null)
            {
                // Redireciona para o URL de retorno SE for um URL local (por segurança)
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Se não houver URL de retorno, redireciona para uma página padrão.
                // Normalmente redireciona para a página inicial da aplicação principal.
                // Certifique-se que o Page "/Index" existe ou use RedirectToAction.
                // return RedirectToPage("/Index"); // Pode precisar ajustar isto
                return RedirectToAction("Index", "Home"); // Mais comum para ir para a raiz da app
            }
        }
    }
}
