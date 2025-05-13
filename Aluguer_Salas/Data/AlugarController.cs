using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Aluguer_Salas.Controllers
{
    [Authorize]
    public class AlugarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlugarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Salas()
        {
            if (_context.Salas == null)
            {
                return Problem("O conjunto de entidades 'ApplicationDbContext.Salas' é nulo.");
            }
            List<Sala> model = await _context.Salas.AsNoTracking().ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> Aluguer(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var salaParaAlugar = await _context.Salas.FindAsync(id);

            if (salaParaAlugar == null)
            {
                return NotFound();
            }

            if (!salaParaAlugar.Disponivel)
            {
                TempData["MensagemErro"] = $"A sala '{salaParaAlugar.NomeSala}' não está disponível para reserva no momento.";
                return RedirectToAction("Salas");
            }

            var viewModel = new AluguerViewModel
            {
                SalaId = salaParaAlugar.Id,
                NomeSala = salaParaAlugar.NomeSala,
                Capacidade = salaParaAlugar.Capacidade,
                DescricaoSala = salaParaAlugar.Descricao,
                Data = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarAluguer(AluguerViewModel viewModel)
        {
            DateTime inicioReserva = viewModel.Data.Date.Add(viewModel.HoraInicio.TimeOfDay);
            DateTime fimReserva = viewModel.Data.Date.Add(viewModel.HoraFim.TimeOfDay);

            if (fimReserva <= inicioReserva)
            {
                ModelState.AddModelError("HoraFim", "A hora de fim deve ser posterior à hora de início.");
            }
            if (inicioReserva < DateTime.Now)
            {
                ModelState.AddModelError("HoraInicio", "A data/hora de início não pode ser no passado.");
            }

            var sala = await _context.Salas.FindAsync(viewModel.SalaId);
            if (sala == null)
            {
                ModelState.AddModelError("", "Sala não encontrada. A reserva não pode ser concluída.");
            }

            if (sala != null)
            {
                bool conflito = await _context.Reservas
                    .AnyAsync(r => r.IdSala == viewModel.SalaId &&
                                   r.Status != "Cancelada" &&
                                   ((inicioReserva >= r.HoraInicio && inicioReserva < r.HoraFim) ||
                                    (fimReserva > r.HoraInicio && fimReserva <= r.HoraFim) ||
                                    (inicioReserva <= r.HoraInicio && fimReserva >= r.HoraFim)));
                if (conflito)
                {
                    ModelState.AddModelError("", "Já existe uma reserva para esta sala no horário selecionado. Por favor, escolha outro horário.");
                }
            }


            ModelState.Remove("NomeSala");
            ModelState.Remove("Capacidade"); // Adicione este se "Capacidade" também estiver a dar erro
            ModelState.Remove("DescricaoSala");
          

            if (ModelState.IsValid)
            {
                try
                {
                    var reserva = new Reserva
                    {
                        IdSala = viewModel.SalaId,
                        Data = viewModel.Data.Date,
                        HoraInicio = inicioReserva,
                        HoraFim = fimReserva,
                        Status = "Pendente",
                        UtilizadorIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    };

                    _context.Reservas.Add(reserva);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = $"Reserva para a sala '{sala?.NomeSala}' efetuada com sucesso para {reserva.Data:dd/MM/yyyy} das {reserva.HoraInicio:HH:mm} às {reserva.HoraFim:HH:mm}.";
                    return RedirectToAction("Salas");
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Não foi possível guardar a reserva devido a um problema na base de dados. Tente novamente.");
                }
            }

            if (sala != null)
            {
                viewModel.NomeSala = sala.NomeSala;
                viewModel.Capacidade = sala.Capacidade;
                viewModel.DescricaoSala = sala.Descricao;
            }
            else if (viewModel.SalaId > 0)
            {
                var salaOriginal = await _context.Salas.AsNoTracking().FirstOrDefaultAsync(s => s.Id == viewModel.SalaId);
                if (salaOriginal != null)
                {
                    viewModel.NomeSala = salaOriginal.NomeSala;
                    viewModel.Capacidade = salaOriginal.Capacidade;
                    viewModel.DescricaoSala = salaOriginal.Descricao;
                }
            }

            return View("Aluguer", viewModel);
        }
    }
}