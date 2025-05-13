using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims; // Necessário para User.FindFirstValue

namespace Aluguer_Salas.Controllers
{
    [Authorize]
    public class AlugarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlugarController> _logger; // Adicionado para logging

        public AlugarController(ApplicationDbContext context, ILogger<AlugarController> logger) // Adicionado ILogger
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Alugar/Salas
        public async Task<IActionResult> Salas()
        {
            List<Sala> model;
            if (_context.Salas != null)
            {
                model = await _context.Salas.AsNoTracking().ToListAsync();
            }
            else
            {
                model = new List<Sala>();
                _logger.LogWarning("_context.Salas é nulo ao tentar carregar a lista de salas.");
                TempData["ErrorMessage"] = "Não foi possível carregar as salas no momento.";
            }
            return View(model);
        }

        // GET: /Alugar/Aluguer/{id}
        public async Task<IActionResult> Aluguer(int id)
        {
            if (id == 0)
            {
                _logger.LogWarning("Tentativa de aluguer com ID de sala zero.");
                return NotFound();
            }

            var salaParaAlugar = await _context.Salas.FindAsync(id);

            if (salaParaAlugar == null)
            {
                _logger.LogWarning($"Sala com ID {id} não encontrada para aluguer.");
                TempData["ErrorMessage"] = "Sala não encontrada.";
                return RedirectToAction(nameof(Salas));
            }

            if (!salaParaAlugar.Disponivel)
            {
                TempData["MensagemErro"] = $"A sala '{salaParaAlugar.NomeSala}' não está disponível para reserva no momento.";
                return RedirectToAction(nameof(Salas));
            }

            var viewModel = new AluguerViewModel
            {
                SalaId = salaParaAlugar.Id,
                NomeSala = salaParaAlugar.NomeSala,
                Capacidade = salaParaAlugar.Capacidade,
                DescricaoSala = salaParaAlugar.Descricao,
                Data = DateTime.Today,
                // Define valores padrão para HoraInicio e HoraFim se o teu AluguerViewModel tiver essas propriedades
                // e forem do tipo TimeSpan ou se quiseres pré-definir horários.
                // Exemplo se HoraInicio/HoraFim no ViewModel forem TimeSpan:
                // HoraInicio = new TimeSpan(9, 0, 0), // 09:00
                // HoraFim = new TimeSpan(10, 0, 0)   // 10:00
                // Se forem DateTime no ViewModel, podes inicializá-las aqui ou deixar que o input type="time" faça o seu trabalho.
                // O ideal é que HoraInicio e HoraFim no ViewModel sejam apenas para input e a combinação com a data
                // seja feita no POST, como já está a ser feito.
            };

            return View(viewModel);
        }

        // POST: /Alugar/ConfirmarAluguer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarAluguer(AluguerViewModel viewModel)
        {
            // Remove o estado das propriedades que são apenas para exibição
            // e não devem ser validadas neste POST, pois vêm do GET e o fieldset está disabled.
            ModelState.Remove("NomeSala");
            ModelState.Remove("DescricaoSala");
            ModelState.Remove("Capacidade");

          
            DateTime inicioReserva = viewModel.Data.Date.Add(viewModel.HoraInicio.TimeOfDay);
            DateTime fimReserva = viewModel.Data.Date.Add(viewModel.HoraFim.TimeOfDay);

            if (fimReserva <= inicioReserva)
            {
                ModelState.AddModelError(nameof(viewModel.HoraFim), "A hora de fim deve ser posterior à hora de início.");
            }

            // Considera verificar se a data é hoje ou no futuro, e se a hora de início já passou para hoje.
            if (inicioReserva < DateTime.Now)
            {
                ModelState.AddModelError(nameof(viewModel.HoraInicio), "A data/hora de início da reserva não pode ser no passado.");
            }

            var sala = await _context.Salas.FindAsync(viewModel.SalaId);
            if (sala == null)
            {
                ModelState.AddModelError("", "Sala não encontrada. A reserva não pode ser concluída.");
            }
            else if (!sala.Disponivel) // Verifica novamente a disponibilidade no momento do POST
            {
                ModelState.AddModelError("", $"A sala '{sala.NomeSala}' não está mais disponível.");
            }

            // Lógica para verificar conflitos de horário com outras reservas
            if (sala != null && _context.Reservas != null)
            {
                bool conflito = await _context.Reservas
                    .AnyAsync(r => r.IdSala == viewModel.SalaId &&
                                   r.Status != "Cancelada" && // Considera outros status que não sejam conflito
                                   ((inicioReserva >= r.HoraInicio && inicioReserva < r.HoraFim) || // Nova reserva começa durante uma existente
                                    (fimReserva > r.HoraInicio && fimReserva <= r.HoraFim) ||    // Nova reserva termina durante uma existente
                                    (inicioReserva <= r.HoraInicio && fimReserva >= r.HoraFim))); // Nova reserva engloba uma existente
                if (conflito)
                {
                    ModelState.AddModelError("", "Já existe uma reserva para esta sala no horário selecionado. Por favor, escolha outro horário.");
                }
            }
            // --- FIM DA LÓGICA DE VALIDAÇÃO PERSONALIZADA ---


            if (ModelState.IsValid)
            {
                try
                {
                    var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(utilizadorId))
                    {
                        _logger.LogWarning("Utilizador não autenticado tentou confirmar aluguer.");
                        // Normalmente, o [Authorize] no controller já deveria ter prevenido isto.
                        // Mas é uma verificação extra.
                        return Challenge(); // Ou redirecionar para o login
                    }

                    var reserva = new Reserva
                    {
                        IdSala = viewModel.SalaId,
                        // IMPLEMENTA AQUI: Se tens uma entidade Utente separada e precisas do Id dela,
                        // terias de buscar o Utente.Id com base no utilizadorId.
                        // Ex: var utente = await _context.Utentes.FirstOrDefaultAsync(u => u.UtilizadorIdentityId == utilizadorId);
                        // IdUtente = utente?.Id, (precisa de tratamento se utente for null)

                        UtilizadorIdentityId = utilizadorId, // Se guardas o ID do IdentityUser diretamente
                        Data = viewModel.Data.Date,      // Guarda apenas a data
                        HoraInicio = inicioReserva,      // DateTime combinado
                        HoraFim = fimReserva,        // DateTime combinado
                        Status = "Confirmada"              // Ou "Confirmada" se não houver processo de aprovação
                    };

                    if (_context.Reservas == null)
                    {
                        _logger.LogError("_context.Reservas é nulo. Não é possível adicionar a reserva.");
                        ModelState.AddModelError("", "Erro no sistema ao tentar aceder ao repositório de reservas.");
                    }
                    else
                    {
                        _context.Reservas.Add(reserva);
                        await _context.SaveChangesAsync();

                        TempData["MensagemSucesso"] = $"Reserva para a sala '{sala?.NomeSala}' efetuada com sucesso para {reserva.Data:dd/MM/yyyy} das {reserva.HoraInicio:HH:mm} às {reserva.HoraFim:HH:mm}.";
                        return RedirectToAction(nameof(Salas));
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Erro de base de dados ao guardar a reserva.");
                    ModelState.AddModelError("", "Não foi possível guardar a reserva devido a um problema na base de dados. Tente novamente.");
                }
                catch (Exception ex) // Captura geral para outros erros inesperados
                {
                    _logger.LogError(ex, "Erro inesperado ao confirmar a reserva.");
                    ModelState.AddModelError("", "Ocorreu um erro inesperado. Tente novamente.");
                }
            }

            // Se ModelState NÃO é válido (ou se houve uma exceção no try-catch que invalidou o ModelState):
            // Recarrega os detalhes da sala para o viewModel antes de retornar à View
            // Isto é importante porque o fieldset disabled não envia estes valores no POST
            if (viewModel.SalaId > 0 && sala == null) // Se 'sala' não foi carregado acima, carrega agora
            {
                sala = await _context.Salas.AsNoTracking().FirstOrDefaultAsync(s => s.Id == viewModel.SalaId);
            }

            if (sala != null)
            {
                viewModel.NomeSala = sala.NomeSala;
                viewModel.Capacidade = sala.Capacidade;
                viewModel.DescricaoSala = sala.Descricao;
            }
            else if (viewModel.SalaId > 0) // Se SalaId foi fornecido mas a sala não foi encontrada
            {
                ModelState.AddModelError("", "Erro: Sala original não encontrada para repopular o formulário.");
                // O ideal seria não chegar aqui se a validação da sala acima funcionou.
            }
            else // Se SalaId não veio no viewModel
            {
                ModelState.AddModelError("", "Erro: ID da sala não fornecido ao submeter o formulário.");
            }

            return View("Aluguer", viewModel);
        }
    }
}