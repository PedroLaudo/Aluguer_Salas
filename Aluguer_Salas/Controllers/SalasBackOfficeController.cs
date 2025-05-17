// Ficheiro: Controllers/SalasBackofficeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Aluguer_Salas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SalasBackofficeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalasBackofficeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SalasBackoffice/Index
        public async Task<IActionResult> Index()
        {
            if (_context.Salas == null)
            {
                TempData["ErrorMessage"] = "Repositório de salas indisponível.";
                return View(new List<Sala>());
            }

            var salas = await _context.Salas.AsNoTracking().ToListAsync();
            return View(salas);
        }

        // GET: SalasBackoffice/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Salas == null)
            {
                TempData["ErrorMessage"] = "Repositório de salas indisponível.";
                return RedirectToAction(nameof(Index));
            }

            var sala = await _context.Salas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (sala == null) return NotFound();
            return View(sala);
        }

        // GET: SalasBackoffice/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SalasBackoffice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeSala,Capacidade,Descricao,Disponivel")] Sala sala)
        {
            ModelState.Remove("Reservas");
            ModelState.Remove("Limpezas");

            if (ModelState.IsValid)
            {
                if (_context.Salas == null)
                {
                    ModelState.AddModelError("", "Não é possível adicionar a sala devido a um erro no sistema.");
                    return View(sala);
                }

                _context.Add(sala);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Sala criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(sala);
        }

        // GET: SalasBackoffice/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Salas == null)
            {
                TempData["ErrorMessage"] = "Repositório de salas indisponível.";
                return RedirectToAction(nameof(Index));
            }

            var sala = await _context.Salas.FindAsync(id);
            if (sala == null) return NotFound();
            return View(sala);
        }

        // POST: SalasBackoffice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NomeSala,Capacidade,Descricao,Disponivel")] Sala sala)
        {
            if (id != sala.Id)
                return NotFound();

            ModelState.Remove("Reservas");
            ModelState.Remove("Limpezas");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sala);
                    await _context.SaveChangesAsync();
                    TempData["MensagemSucesso"] = "Sala atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalaExists(sala.Id))
                        return NotFound();

                    ModelState.AddModelError("", "Os dados foram modificados por outro utilizador. Por favor, recarrega e tenta novamente.");
                    return View(sala);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(sala);
        }

        // GET: SalasBackoffice/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Salas == null)
            {
                TempData["ErrorMessage"] = "Repositório de salas indisponível.";
                return RedirectToAction(nameof(Index));
            }

            var sala = await _context.Salas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (sala == null) return NotFound();

            return View(sala);
        }

        // POST: SalasBackoffice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Salas == null)
            {
                TempData["ErrorMessage"] = "Repositório de salas indisponível.";
                return RedirectToAction(nameof(Index));
            }

            var sala = await _context.Salas.FirstOrDefaultAsync(s => s.Id == id);

            if (sala != null)
            {
                bool temReservas = await _context.Reservas.AnyAsync(r => r.IdSala == id && r.Status != "Cancelada" && r.HoraFim > DateTime.Now);

                if (temReservas)
                {
                    TempData["ErrorMessage"] = $"Não é possível apagar a sala '{sala.NomeSala}' pois existem reservas futuras ou ativas associadas a ela.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _context.Salas.Remove(sala);
                    await _context.SaveChangesAsync();
                    TempData["MensagemSucesso"] = "Sala apagada com sucesso!";
                }
                catch
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro ao tentar apagar a sala.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Sala com ID {id} não encontrada para apagar.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SalaExists(int id)
        {
            if (_context.Salas == null) return false;
            return _context.Salas.Any(e => e.Id == id);
        }
    }
}
