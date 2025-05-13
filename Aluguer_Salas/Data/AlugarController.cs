using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using System.Threading.Tasks;
using System.Linq;      // Adicionado para .Any()
using System.Collections.Generic; // Adicionado para List<T>
using Aluguer_Salas.Models; // Adicionado para Sala

namespace Aluguer_Salas.Controllers
{
    public class AlugarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlugarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Esta Action corresponderá à View "Salas.cshtml"
        // A URL seria /Alugar/Salas
        public async Task<IActionResult> Salas() // O nome da Action DEVE corresponder ao nome da View (ou você pode especificar o nome da view em View("OutroNome"))
        {
            List<Sala> model;
            if (_context.Salas != null)
            {
                model = await _context.Salas.AsNoTracking().ToListAsync();
            }
            else
            {
                model = new List<Sala>(); // Inicializa com lista vazia se _context.Salas for null
            }
            return View(model); // Procura por uma view chamada "Salas.cshtml" em Views/Alugar/
        }

        // Se você quisesse usar "Index.cshtml" como nome da view, a action seria:
        // public async Task<IActionResult> Index()
        // {
        //     var model = await _context.Salas.AsNoTracking().ToListAsync();
        //     return View(model); // Procura por Index.cshtml em Views/Alugar/
        // }
    }
}