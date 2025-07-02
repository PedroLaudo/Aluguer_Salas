using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using System.Threading.Tasks; 

namespace Aluguer_Salas.Controllers
{
    [Authorize(Roles = "Administrador")] // << IMPORTANTE: Restringe o acesso a todo o controller
    public class MaterialBackOfficeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialBackOfficeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Material
        // Este método retorna uma lista de materiais disponíveis no sistema.
        public async Task<IActionResult> Index()
        {
            
            return View(await _context.Materiais.ToListAsync());
        }

        // GET: Material/Details/5
        // Este método retorna os detalhes de um material específico.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materiais
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // GET: Material/Create
        // Este método retorna a view para criar um novo material.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Material/Create
        // Este método processa a criação de um novo material.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,QuantidadeDisponivel")] Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Add(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material adicionado com sucesso!"; 
                return RedirectToAction(nameof(Index));
            }
            return View(material);
        }

        // GET: Material/Edit
        // // Este método retorna a view para editar um material existente.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materiais.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        // POST: Material/Edit/5
        // Este método recebe os dados editado, valida e insere na base de dados.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,QuantidadeDisponivel")] Material material)
        {
            if (id != material.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Material atualizado com sucesso!"; // Mensagem de feedback
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(material);
        }

        // GET: Material/Delete/5
        // Este método retorna a view para confirmar a exclusão de um material.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materiais
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Material/Delete/5
        // Este método processa a exclusão de um material.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materiais.FindAsync(id);
            if (material != null)
            {
                _context.Materiais.Remove(material);
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Material eliminado com sucesso!"; 
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Materiais.Any(e => e.Id == id);
        }
    }
}