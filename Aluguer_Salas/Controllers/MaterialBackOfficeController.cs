using Microsoft.AspNetCore.Authorization; // << Adicionar este using
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using System.Threading.Tasks; // Adicionar se não estiver lá

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
        public async Task<IActionResult> Index()
        {
            // O nome do DbSet aqui deve corresponder ao que você definiu no ApplicationDbContext
            return View(await _context.Materiais.ToListAsync());
        }

        // GET: Material/Details/5
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Material/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,QuantidadeDisponivel")] Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Add(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material adicionado com sucesso!"; // Mensagem de feedback
                return RedirectToAction(nameof(Index));
            }
            return View(material);
        }

        // GET: Material/Edit/5
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
            TempData["SuccessMessage"] = "Material eliminado com sucesso!"; // Mensagem de feedback
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Materiais.Any(e => e.Id == id);
        }
    }
}