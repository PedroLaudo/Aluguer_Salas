using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Adicionar para ToListAsync e DbContext
using System.Collections.Generic;
using System.Threading.Tasks;     // Adicionar para Task
using Aluguer_Salas.Data;         // Adicionar para ApplicationDbContext
using Aluguer_Salas.Models;       // Adicionar para Sala (se não estiver usando ViewModel aqui)
using Microsoft.AspNetCore.Authorization; // Adicionar para Authorize
using System.Linq;                // Adicionar para Select e outros LINQ se usar ViewModel

namespace Aluguer_Salas.Areas.Identity.Pages.Backoffice
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mude de SalaViewModel para o seu modelo Sala diretamente se não precisar de um ViewModel aqui
        // public IList<SalaViewModel> SalasList { get; set; } = new List<SalaViewModel>();
        public IList<Sala> SalasList { get; set; } = new List<Sala>();


        public async Task OnGetAsync()
        {
            if (_context.Salas != null)
            {
                // Se estiver usando o modelo Sala diretamente:
                SalasList = await _context.Salas.ToListAsync();

                // Se você decidir usar SalaViewModel:
                /*
                SalasList = await _context.Salas
                                    .Select(s => new SalaViewModel
                                    {
                                        Id = s.Id,
                                        NomeSala = s.NomeSala,
                                        Capacidade = s.Capacidade,
                                        Descricao = s.Descricao
                                        // Mapeie outras propriedades se necessário
                                    })
                                    .ToListAsync();
                */
            }
        }
    }

    // Se você decidir usar SalaViewModel, mantenha esta definição ou mova para uma pasta ViewModels
    /*
    public class SalaViewModel
    {
        public int Id { get; set; }
        public string NomeSala { get; set; }
        public int Capacidade { get; set; }
        public string Descricao { get; set; }
        // public bool Disponivel { get; set; } // Se relevante
    }
    */
}