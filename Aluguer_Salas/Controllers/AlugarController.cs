// Ficheiro: Controllers/AlugarController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Adicionado

namespace Aluguer_Salas.Controllers
{
    [Authorize]
    public class AlugarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlugarController> _logger;

        public AlugarController(ApplicationDbContext context, ILogger<AlugarController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Alugar/Salas
        public async Task<IActionResult> Salas()
        {
            if (_context.Salas == null)
            {
                TempData["ErrorMessage"] = "Repositório de salas indisponível.";
                return View(new List<Sala>());
            }
            var salas = await _context.Salas.AsNoTracking().Where(s => s.Disponivel).ToListAsync();
            return View(salas);
        }

        // GET: /Alugar/Aluguer/{id}
        // Esta ação prepara o formulário para alugar uma sala específica.
        public async Task<IActionResult> Aluguer(int id)
        {
            if (id == 0)
            {
                _logger.LogWarning("Aluguer GET: Tentativa com ID de sala zero.");
                return NotFound();
            }

            var salaParaAlugar = await _context.Salas.FindAsync(id);

            if (salaParaAlugar == null)
            {
                _logger.LogWarning($"Aluguer GET: Sala com ID {id} não encontrada.");
                TempData["ErrorMessage"] = "Sala não encontrada.";
                return RedirectToAction(nameof(Salas));
            }

            if (!salaParaAlugar.Disponivel)
            {
                TempData["ErrorMessage"] = $"A sala '{salaParaAlugar.NomeSala}' não está disponível para reserva no momento.";
                return RedirectToAction(nameof(Salas));
            }

            var viewModel = new AluguerViewModel
            {
                SalaId = salaParaAlugar.Id,
                NomeSala = salaParaAlugar.NomeSala,
                Capacidade = salaParaAlugar.Capacidade,
                DescricaoSala = salaParaAlugar.Descricao,
                Data = DateTime.Today,
                HorariosOcupados = await GetHorariosOcupadosParaView(salaParaAlugar.Id, DateTime.Today)
            };

            return View(viewModel); // Assume que a sua view se chama "Aluguer.cshtml"
        }

        // POST: /Alugar/Aluguer  (ou /Alugar/ConfirmarAluguer se mantiver essa action no form)
        // Esta ação trata da submissão do formulário.
        // Pode ser chamada para atualizar horários (se a data mudar) ou para confirmar a reserva.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aluguer(AluguerViewModel viewModel, string? command) // 'command' para distinguir ações
        {
            var sala = await _context.Salas.FindAsync(viewModel.SalaId);
            if (sala == null)
            {
                ModelState.AddModelError("", "Sala não encontrada. A operação não pode ser concluída.");
                // Repopular o modelo para a view mesmo com erro, se possível
                viewModel.HorariosOcupados = await GetHorariosOcupadosParaView(viewModel.SalaId, viewModel.Data);
                return View(viewModel);
            }

            // Repopular detalhes da sala no viewModel, pois eles vêm desabilitados do formulário
            viewModel.NomeSala = sala.NomeSala;
            viewModel.Capacidade = sala.Capacidade;
            viewModel.DescricaoSala = sala.Descricao;
            viewModel.HorariosOcupados = await GetHorariosOcupadosParaView(viewModel.SalaId, viewModel.Data);

            // Se o comando é para confirmar a reserva (ou se 'command' for nulo e for a submissão padrão)
            if (string.IsNullOrEmpty(command) || command.Equals("ConfirmarReserva", StringComparison.OrdinalIgnoreCase))
            {
                // Validações
                if (viewModel.Data.Date < DateTime.Today)
                {
                    ModelState.AddModelError(nameof(viewModel.Data), "A data da reserva não pode ser no passado.");
                }
                if (viewModel.HoraFim <= viewModel.HoraInicio)
                {
                    ModelState.AddModelError(nameof(viewModel.HoraFim), "A hora de fim deve ser posterior à hora de início.");
                }

                DateTime inicioReserva = viewModel.Data.Date.Add(viewModel.HoraInicio);
                DateTime fimReserva = viewModel.Data.Date.Add(viewModel.HoraFim);

                if (inicioReserva < DateTime.Now)
                {
                    ModelState.AddModelError(nameof(viewModel.HoraInicio), "A data/hora de início da reserva não pode ser no passado.");
                }

                if (!sala.Disponivel)
                {
                    ModelState.AddModelError("", $"A sala '{sala.NomeSala}' não está mais disponível.");
                }

                if (ModelState.IsValid && _context.Reservas != null) // Verifica ModelState ANTES da query de conflito
                {
                    bool conflito = await _context.Reservas
                        .AnyAsync(r => r.IdSala == viewModel.SalaId &&
                                       r.Status != "Cancelada" &&
                                       ((inicioReserva >= r.HoraInicio && inicioReserva < r.HoraFim) ||
                                        (fimReserva > r.HoraInicio && fimReserva <= r.HoraFim) ||
                                        (inicioReserva <= r.HoraInicio && fimReserva >= r.HoraFim)));
                    if (conflito)
                    {
                        ModelState.AddModelError("", $"A sala '{sala.NomeSala}' já está reservada para o período de {inicioReserva:dd/MM/yyyy HH:mm} a {fimReserva:HH:mm}. Por favor, escolha outro horário.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(utilizadorId))
                    {
                        return Challenge();
                    }

                    var reserva = new Reserva
                    {
                        IdSala = viewModel.SalaId,
                        UtilizadorIdentityId = utilizadorId,
                        Data = viewModel.Data.Date,
                        HoraInicio = inicioReserva,
                        HoraFim = fimReserva,
                        Status = "Confirmada"
                    };

                    if (_context.Reservas == null)
                    {
                        _logger.LogError("ConfirmarAluguer POST: _context.Reservas é nulo.");
                        ModelState.AddModelError("", "Erro no sistema ao tentar aceder ao repositório de reservas.");
                        return View(viewModel);
                    }

                    try
                    {
                        _context.Reservas.Add(reserva);
                        await _context.SaveChangesAsync();
                        TempData["MensagemSucesso"] = $"Reserva para a sala '{sala.NomeSala}' efetuada com sucesso para {reserva.Data:dd/MM/yyyy} das {reserva.HoraInicio:HH:mm} às {reserva.HoraFim:HH:mm}.";
                        return RedirectToAction(nameof(Salas)); // Ou para uma página de confirmação/minhas reservas
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "ConfirmarAluguer POST: Erro de DbUpdateException ao guardar reserva.");
                        ModelState.AddModelError("", "Não foi possível guardar a reserva. Tente novamente.");
                    }
                }
            }
            // Se não foi para confirmar, ou se ModelState é inválido, retorna a view com o modelo (e horários atualizados)
            return View(viewModel);
        }

        // Método auxiliar para buscar horários ocupados para a view
        private async Task<List<HorarioOcupadoViewModel>> GetHorariosOcupadosParaView(int salaId, DateTime dataSelecionada)
        {
            if (_context.Reservas == null) return new List<HorarioOcupadoViewModel>();

            return await _context.Reservas
                .Where(r => r.IdSala == salaId && r.Data.Date == dataSelecionada.Date && r.Status != "Cancelada")
                .OrderBy(r => r.HoraInicio)
                .Select(r => new HorarioOcupadoViewModel
                {
                    HoraInicio = r.HoraInicio.TimeOfDay, // Pega apenas a parte da hora
                    HoraFim = r.HoraFim.TimeOfDay      // Pega apenas a parte da hora
                })
                .ToListAsync();
        }

        // Se precisar de uma action específica para obter horários via AJAX (RECOMENDADO para melhor UX)
        [HttpGet]
        public async Task<IActionResult> GetHorariosOcupadosJson(int salaId, DateTime data)
        {
            if (salaId == 0 || data == DateTime.MinValue)
            {
                return BadRequest("Parâmetros inválidos.");
            }
            var horarios = await GetHorariosOcupadosParaView(salaId, data);
            return Json(horarios);
        }
    }
}