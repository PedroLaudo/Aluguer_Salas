using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Aluguer_Salas.Areas.Identity.Pages.Backoffice
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] // Liga a Sala que vem do formul�rio (via Id oculto) no POST
        public Sala Sala { get; set; } = default!;

        // M�todo para buscar e exibir os dados da sala a ser apagada (GET)
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sala = await _context.Salas.FirstOrDefaultAsync(m => m.Id == id);

            if (sala == null)
            {
                return NotFound();
            }
            Sala = sala;
            return Page();
        }

        // M�todo para processar a confirma��o de apagar (POST)
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca a sala novamente para garantir que ainda existe antes de apagar
            var salaToDelete = await _context.Salas.FindAsync(id);

            if (salaToDelete != null)
            {
                Sala = salaToDelete; // Atribui para poder mostrar info se houver erro ou redirecionar
                _context.Salas.Remove(Sala); // Marca a sala para remo��o
                await _context.SaveChangesAsync(); // Salva as altera��es (remove do banco)
            }
            // Se n�o encontrar, n�o h� nada a fazer, apenas redireciona

            return RedirectToPage("./Index"); // Redireciona para a lista ap�s apagar
        }
    }
}