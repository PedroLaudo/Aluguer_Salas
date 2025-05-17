// Ficheiro: Controllers/PerfilController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Aluguer_Salas.Models;
using Aluguer_Salas.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
// Removido: using Aluguer_Salas.ViewModels;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Collections.Generic; // Para List<Reserva>

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

        // GET: /Perfil ou /Perfil/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                var currentUserId = _userManager.GetUserId(User);
                _logger.LogWarning("Perfil/Index GET: Utilizador não encontrado com ID: {UserId}", currentUserId ?? "não disponível");
                TempData["ErrorMessageController"] = "Não foi possível carregar os seus dados de perfil."; // Usar chave diferente para TempData
                return RedirectToAction("Index", "Home");
            }

            // Coloca o utilizador no ViewData
            ViewData["CurrentUtilizador"] = user;

            var minhasReservas = await _context.Reservas
                                       .Where(r => r.UtilizadorIdentityId == user.Id)
                                       .Include(r => r.Sala)
                                       .OrderByDescending(r => r.Data)
                                       .ThenByDescending(r => r.HoraInicio)
                                       .ToListAsync();

            // Passa mensagens TempData diretamente para ViewData para a view ler
            if (TempData.ContainsKey("StatusMessageController"))
                ViewData["StatusMessage"] = TempData["StatusMessageController"]?.ToString();
            if (TempData.ContainsKey("ErrorMessageController"))
                ViewData["ErrorMessage"] = TempData["ErrorMessageController"]?.ToString();

            // Passa a lista de reservas como o modelo principal da view
            return View("Perfil", minhasReservas);
        }

        // POST: /Perfil/CancelarReserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarReserva(int idReserva)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Perfil/CancelarReserva POST: Utilizador não identificado.");
                TempData["ErrorMessageController"] = "Não foi possível identificar o utilizador.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Perfil/CancelarReserva POST: Utilizador com ID {UserId} não encontrado.", userId);
                TempData["ErrorMessageController"] = "Utilizador não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var reservaParaRemover = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.UtilizadorIdentityId == user.Id);

            if (reservaParaRemover == null)
            {
                _logger.LogWarning("Perfil/CancelarReserva POST: Reserva {IdReserva} não encontrada ou não pertence ao utilizador {UserId}.", idReserva, user.Id);
                TempData["ErrorMessageController"] = "Reserva não encontrada ou não tem permissão para esta ação.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Reservas.Remove(reservaParaRemover);
                await _context.SaveChangesAsync();
                TempData["StatusMessageController"] = "Reserva removida com sucesso!";
                _logger.LogInformation("Perfil/CancelarReserva POST: Reserva {IdReserva} REMOVIDA pelo utilizador {UserId} ({UserName}).", idReserva, user.Id, user.UserName);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Perfil/CancelarReserva POST: Erro ao REMOVER reserva {IdReserva} para {UserId}.", idReserva, user.Id);
                TempData["ErrorMessageController"] = "Ocorreu um erro ao tentar remover a reserva. Tente novamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}