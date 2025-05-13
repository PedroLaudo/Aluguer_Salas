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
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Sala Sala { get; set; } = default!; // A sala a ser exibida

        public async Task<IActionResult> OnGetAsync(int? id) // Recebe o ID da sala pela rota
        {
            if (id == null)
            {
                return NotFound(); // Se nenhum ID for fornecido
            }

            // Busca a sala no banco de dados pelo ID
            // Incluir .AsNoTracking() é uma otimização, já que não vamos modificar os dados aqui.
            var sala = await _context.Salas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (sala == null)
            {
                return NotFound(); // Se a sala com o ID fornecido não for encontrada
            }

            Sala = sala; // Atribui a sala encontrada à propriedade do modelo
            return Page(); // Exibe a página
        }
    }
}