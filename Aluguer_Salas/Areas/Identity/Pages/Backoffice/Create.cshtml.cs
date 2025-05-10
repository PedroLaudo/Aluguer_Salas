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

        // BindProperty liga os dados do formulário a esta propriedade
        [BindProperty]
        public Sala Sala { get; set; } = default!; // A sala a ser criada

        // Método para exibir o formulário (GET)
        public IActionResult OnGet()
        {
            // Pode pré-definir valores aqui se necessário
            // Sala = new Sala { Disponivel = true };
            return Page();
        }

        // Método para processar o formulário (POST)
        public async Task<IActionResult> OnPostAsync()
        {
            // Remove validações que não se aplicam na criação (se houver)
            ModelState.Remove("Sala.Reservas");
            ModelState.Remove("Sala.Disponibilidades");

            if (!ModelState.IsValid)
            {
                return Page(); // Se o modelo não for válido, reexibe a página com os erros
            }

            _context.Salas.Add(Sala); // Adiciona a nova sala ao DbContext
            await _context.SaveChangesAsync(); // Salva as alterações no banco de dados

            return RedirectToPage("./Index"); // Redireciona para a lista após criar
        }
    }
}