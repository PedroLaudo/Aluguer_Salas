
using Aluguer_Salas.Data; 
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using Aluguer_Salas.Models;


namespace Aluguer_Salas.Areas.Identity.Pages.Account
{
    public class SalasModel : PageModel
    {
        private readonly ApplicationDbContext _context; // Supondo que seu DbContext se chama ApplicationDbContext

        public SalasModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Sala> ListaSalas { get; set; } = new List<Sala>();

        public async Task OnGetAsync()
        {
            // Carrega todas as salas sem filtrar por 'Disponivel'
            ListaSalas = await _context.Salas.ToListAsync();
        }
    }
}