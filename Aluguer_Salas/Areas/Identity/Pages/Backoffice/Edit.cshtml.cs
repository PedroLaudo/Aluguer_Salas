using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Aluguer_Salas.Areas.Identity.Pages.Backoffice
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] // Liga os dados do formul�rio a esta propriedade no POST
        public Sala Sala { get; set; } = default!;

        // M�todo para buscar e exibir os dados da sala no formul�rio (GET)
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

        // M�todo para processar os dados editados do formul�rio (POST)
        public async Task<IActionResult> OnPostAsync()
        {
            // Remove valida��es que n�o se aplicam na edi��o (se houver)
            ModelState.Remove("Sala.Reservas");
            ModelState.Remove("Sala.Disponibilidades");

            if (!ModelState.IsValid)
            {
                return Page(); // Reexibe o formul�rio com erros se inv�lido
            }

            // Marca a entidade como modificada
            _context.Attach(Sala).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // Tenta salvar as altera��es
            }
            catch (DbUpdateConcurrencyException) // Trata erros de concorr�ncia (algu�m alterou enquanto voc� editava)
            {
                if (!SalaExists(Sala.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Relan�a a exce��o se for outro erro
                }
            }

            return RedirectToPage("./Index"); // Redireciona para a lista ap�s editar
        }

        // Helper para verificar se a sala ainda existe
        private bool SalaExists(int id)
        {
            return _context.Salas.Any(e => e.Id == id);
        }
    }
}