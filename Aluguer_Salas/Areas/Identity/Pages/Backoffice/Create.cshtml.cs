using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
namespace Aluguer_Salas.Areas.Identity.Pages.Backoffice
{
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // BindProperty liga os dados do formul�rio a esta propriedade
        [BindProperty]
        public Sala Sala { get; set; } = default!; // A sala a ser criada

        // M�todo para exibir o formul�rio (GET)
        public IActionResult OnGet()
        {
            // Pode pr�-definir valores aqui se necess�rio
            // Sala = new Sala { Disponivel = true };
            return Page();
        }

        // M�todo para processar o formul�rio (POST)
        public async Task<IActionResult> OnPostAsync()
        {
            // Remove valida��es que n�o se aplicam na cria��o (se houver)
            ModelState.Remove("Sala.Reservas");
            ModelState.Remove("Sala.Disponibilidades");

            if (!ModelState.IsValid)
            {
                return Page(); // Se o modelo n�o for v�lido, reexibe a p�gina com os erros
            }

            _context.Salas.Add(Sala); // Adiciona a nova sala ao DbContext
            await _context.SaveChangesAsync(); // Salva as altera��es no banco de dados

            return RedirectToPage("./Index"); // Redireciona para a lista ap�s criar
        }
    }
}