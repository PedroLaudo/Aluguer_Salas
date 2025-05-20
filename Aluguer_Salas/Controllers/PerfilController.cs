// File: Controllers/PerfilController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Aluguer_Salas.Models; // Para Utilizador, Reserva, RequisicaoMaterial, Sala, Material
using Aluguer_Salas.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectList
using System;

namespace Aluguer_Salas.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(
            UserManager<Utilizador> userManager,
            ApplicationDbContext context,
            ILogger<PerfilController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Perfil/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Utilizador não encontrado no Index do PerfilController para {UserName}", User.Identity?.Name);
                TempData["ErrorMessageController"] = "Não foi possível carregar os seus dados de perfil. Tente fazer login novamente.";
                return RedirectToAction("Index", "Home");
            }

            var minhasReservas = await _context.Reservas
                                       .Where(r => r.UtilizadorIdentityId == user.Id)
                                       .Include(r => r.Sala)
                                       .OrderByDescending(r => r.HoraInicio)
                                       .ToListAsync();

            var minhasRequisicoes = await _context.RequisicoesMaterial
                                            .Where(rm => rm.UtilizadorId == user.Id)
                                            .Include(rm => rm.Material)
                                            .OrderByDescending(rm => rm.DataRequisicao)
                                            .ThenByDescending(rm => rm.HoraInicio)
                                            .ToListAsync();

            ViewData["CurrentUtilizador"] = user;
            ViewData["MinhasReservas"] = minhasReservas;
            ViewData["MinhasRequisicoesMaterial"] = minhasRequisicoes;

            if (TempData.ContainsKey("SuccessMessage"))
                ViewData["StatusMessage"] = TempData["SuccessMessage"]?.ToString();
            else if (TempData.ContainsKey("StatusMessageController"))
                ViewData["StatusMessage"] = TempData["StatusMessageController"]?.ToString();

            if (TempData.ContainsKey("ErrorMessageController"))
                ViewData["ErrorMessage"] = TempData["ErrorMessageController"]?.ToString();

            return View("Perfil");
        }

        // POST: Perfil/CancelarReserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarReserva(int idReserva)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessageController"] = "Utilizador não autenticado.";
                return RedirectToAction(nameof(Index));
            }

            var reserva = await _context.Reservas
                                  .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.UtilizadorIdentityId == user.Id);

            if (reserva == null)
            {
                TempData["ErrorMessageController"] = "Reserva não encontrada ou não tem permissão para cancelar.";
                return RedirectToAction(nameof(Index));
            }

            DateTime reservaStartDateTime = reserva.HoraInicio;
            if (reservaStartDateTime <= DateTime.Now)
            {
                TempData["ErrorMessageController"] = "Não é possível cancelar uma reserva que já ocorreu ou está em andamento.";
                return RedirectToAction(nameof(Index));
            }

            if (!(reserva.Status?.Equals("Confirmada", StringComparison.OrdinalIgnoreCase) == true ||
                  reserva.Status?.Equals("Pendente", StringComparison.OrdinalIgnoreCase) == true))
            {
                TempData["ErrorMessageController"] = "Apenas reservas com estado 'Confirmada' ou 'Pendente' podem ser canceladas.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                reserva.Status = "Cancelada";
                _context.Update(reserva);
                await _context.SaveChangesAsync();
                TempData["StatusMessageController"] = "Reserva cancelada com sucesso!";
                _logger.LogInformation("Reserva {IdReserva} CANCELADA pelo utilizador {UserId}.", idReserva, user.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao CANCELAR reserva {IdReserva} para o utilizador {UserId}.", idReserva, user.Id);
                TempData["ErrorMessageController"] = "Ocorreu um erro ao tentar cancelar a reserva.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Perfil/CancelarRequisicaoMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarRequisicaoMaterial(int idRequisicao)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessageController"] = "Utilizador não identificado.";
                return RedirectToAction(nameof(Index));
            }

            var requisicaoParaCancelar = await _context.RequisicoesMaterial
                .FirstOrDefaultAsync(r => r.Id == idRequisicao && r.UtilizadorId == userId);

            if (requisicaoParaCancelar == null)
            {
                TempData["ErrorMessageController"] = "Requisição de material não encontrada ou não tem permissão para esta ação.";
                return RedirectToAction(nameof(Index));
            }

            DateTime requisicaoStartDateTime = requisicaoParaCancelar.DataRequisicao.Add(requisicaoParaCancelar.HoraInicio);
            if (requisicaoStartDateTime <= DateTime.Now)
            {
                TempData["ErrorMessageController"] = "Esta requisição de material já passou ou está em curso e não pode ser cancelada.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.RequisicoesMaterial.Remove(requisicaoParaCancelar);
                await _context.SaveChangesAsync();
                TempData["StatusMessageController"] = "Requisição de material cancelada com sucesso!";
                _logger.LogInformation("Requisição de Material {IdRequisicao} CANCELADA pelo utilizador {UserId}.", idRequisicao, userId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao CANCELAR requisição de material {IdRequisicao} para {UserId}.", idRequisicao, userId);
                TempData["ErrorMessageController"] = "Ocorreu um erro ao tentar cancelar a requisição de material.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Perfil/EditarRequisicaoMaterial/5
        public async Task<IActionResult> EditarRequisicaoMaterial(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessageController"] = "ID da requisição não fornecido.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var requisicaoMaterial = await _context.RequisicoesMaterial
                                             .Include(rm => rm.Material)
                                             .FirstOrDefaultAsync(m => m.Id == id && m.UtilizadorId == userId);

            if (requisicaoMaterial == null)
            {
                TempData["ErrorMessageController"] = "Requisição de material não encontrada ou não tem permissão para editar.";
                return RedirectToAction(nameof(Index));
            }

            DateTime requisicaoStartDateTime = requisicaoMaterial.DataRequisicao.Add(requisicaoMaterial.HoraInicio);
            if (requisicaoStartDateTime <= DateTime.Now)
            {
                TempData["ErrorMessageController"] = "Esta requisição de material já passou ou está em curso e não pode ser editada.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ListaMateriaisDisponiveis = new SelectList(
                await _context.Materiais.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", requisicaoMaterial.MaterialId
            );
            ViewData["Title"] = "Editar Requisição de Material";
            return View(requisicaoMaterial);
        }

        // POST: Perfil/EditarRequisicaoMaterial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRequisicaoMaterial(int id,
            [Bind("Id,UtilizadorId,MaterialId,QuantidadeRequisitada,DataRequisicao,HoraInicio,HoraFim")] RequisicaoMaterial requisicaoMaterialEditada)
        {
            if (id != requisicaoMaterialEditada.Id)
            {
                TempData["ErrorMessageController"] = "ID da requisição não corresponde.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var originalRequisicao = await _context.RequisicoesMaterial
                                                 .Include(rm => rm.Material)
                                                 .FirstOrDefaultAsync(r => r.Id == id);


            if (originalRequisicao == null)
            {
                TempData["ErrorMessageController"] = "Requisição original não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (originalRequisicao.UtilizadorId != userId)
            {
                TempData["ErrorMessageController"] = "Não tem permissão para alterar esta requisição.";
                return RedirectToAction(nameof(Index));
            }
            requisicaoMaterialEditada.UtilizadorId = originalRequisicao.UtilizadorId;
            requisicaoMaterialEditada.Material = originalRequisicao.Material;


            if (requisicaoMaterialEditada.HoraFim <= requisicaoMaterialEditada.HoraInicio)
            {
                ModelState.AddModelError("HoraFim", "A hora de fim deve ser posterior à hora de início.");
            }

            if (requisicaoMaterialEditada.DataRequisicao.Date < DateTime.Today.Date && requisicaoMaterialEditada.DataRequisicao.Date != originalRequisicao.DataRequisicao.Date)
            {
                ModelState.AddModelError("DataRequisicao", "A data de requisição não pode ser no passado.");
            }
            else if (requisicaoMaterialEditada.DataRequisicao.Date == DateTime.Today.Date &&
                     requisicaoMaterialEditada.HoraInicio < DateTime.Now.TimeOfDay &&
                     (requisicaoMaterialEditada.DataRequisicao.Date != originalRequisicao.DataRequisicao.Date || requisicaoMaterialEditada.HoraInicio != originalRequisicao.HoraInicio))
            {
                ModelState.AddModelError("HoraInicio", "A hora de início para hoje não pode ser no passado.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var material = await _context.Materiais.FindAsync(requisicaoMaterialEditada.MaterialId);
                    if (material == null)
                    {
                        ModelState.AddModelError("MaterialId", "Material selecionado inválido.");
                    }
                    else
                    {
                        var conflitos = await _context.RequisicoesMaterial
                            .Where(r => r.Id != requisicaoMaterialEditada.Id &&
                                        r.MaterialId == requisicaoMaterialEditada.MaterialId &&
                                        r.DataRequisicao.Date == requisicaoMaterialEditada.DataRequisicao.Date &&
                                        requisicaoMaterialEditada.HoraInicio < r.HoraFim &&
                                        requisicaoMaterialEditada.HoraFim > r.HoraInicio)
                            .ToListAsync();
                        int jaRequisitadoPorOutros = conflitos.Sum(r => r.QuantidadeRequisitada);
                        int disponivelReal = material.QuantidadeDisponivel - jaRequisitadoPorOutros;

                        if (requisicaoMaterialEditada.QuantidadeRequisitada <= 0)
                        {
                            ModelState.AddModelError("QuantidadeRequisitada", "A quantidade requisitada deve ser maior que zero.");
                        }
                        else if (requisicaoMaterialEditada.QuantidadeRequisitada > disponivelReal)
                        {
                            ModelState.AddModelError("QuantidadeRequisitada", $"Apenas {Math.Max(0, disponivelReal)} unidades de '{material.Nome}' estão disponíveis para este período.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        _context.Entry(originalRequisicao).CurrentValues.SetValues(requisicaoMaterialEditada);
                        await _context.SaveChangesAsync();
                        TempData["StatusMessageController"] = "Requisição de material atualizada com sucesso!";
                        _logger.LogInformation("Requisição de Material {IdRequisicao} ATUALIZADA pelo utilizador {UserId}.", id, userId);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequisicaoMaterialExists(requisicaoMaterialEditada.Id))
                    {
                        TempData["ErrorMessageController"] = "Requisição não encontrada (concorrência).";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Os dados desta requisição foram alterados por outro utilizador. Por favor, reveja e tente novamente.");
                        var currentRequisicao = await _context.RequisicoesMaterial
                                                            .Include(rm => rm.Material)
                                                            .AsNoTracking()
                                                            .FirstOrDefaultAsync(rm => rm.Id == id);
                        if (currentRequisicao != null) requisicaoMaterialEditada = currentRequisicao;
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Erro ao ATUALIZAR requisição de material {IdRequisicao} para {UserId}.", id, userId);
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar atualizar a requisição.");
                }
            }

            ViewBag.ListaMateriaisDisponiveis = new SelectList(
                await _context.Materiais.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", requisicaoMaterialEditada.MaterialId
            );
            ViewData["Title"] = "Editar Requisição de Material";
            return View(requisicaoMaterialEditada);
        }

        private bool RequisicaoMaterialExists(int id)
        {
            return _context.RequisicoesMaterial.Any(e => e.Id == id);
        }

        // O método ReservaExists foi removido pois era usado apenas pelos métodos de EditarReserva.
    }
}