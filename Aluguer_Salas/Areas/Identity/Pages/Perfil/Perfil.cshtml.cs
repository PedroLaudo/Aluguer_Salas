using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Aluguer_Salas.Models; // Certifique-se que este namespace contém Utilizador e Reserva
using Aluguer_Salas.Data;   // Certifique-se que este namespace contém ApplicationDbContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // Para ILogger
using System.Security.Claims; // Para ClaimTypes

namespace Aluguer_Salas.Areas.Identity.Pages.Perfil
{
    [Authorize]
    public class PerfilModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PerfilModel> _logger;

        public PerfilModel(
            UserManager<Utilizador> userManager,
            ApplicationDbContext context,
            ILogger<PerfilModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public Utilizador? CurrentUtilizador { get; set; }
        public IList<Reserva> MinhasReservas { get; set; } = new List<Reserva>();

        [TempData]
        public string? StatusMessage { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User); 
            if (user == null)
            {
                var currentUserId = _userManager.GetUserId(User); // Tenta obter o ID mesmo se GetUserAsync falhar
                ErrorMessage = $"Não foi possível carregar os dados do utilizador (ID: {currentUserId ?? "não disponível"}).";
                _logger.LogWarning("OnGetAsync: Utilizador não encontrado com ID: {UserId}", currentUserId ?? "não disponível");
                return RedirectToPage("/Index"); // Considere uma página de erro mais específica
            }

            CurrentUtilizador = user;

            MinhasReservas = await _context.Reservas
                                       .Where(r => r.UtilizadorIdentityId == user.Id)
                                       .Include(r => r.Sala)
                                       .OrderByDescending(r => r.Data)
                                       .ThenByDescending(r => r.HoraInicio)
                                       .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelarReservaAsync(int idReserva)
        {
            // Tenta obter o ID do utilizador diretamente do ClaimsPrincipal
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Ou _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Não foi possível identificar o utilizador. Ação não permitida.";
                _logger.LogWarning("OnPostCancelarReservaAsync: Tentativa de ação por utilizador não identificado (ID não encontrado no ClaimsPrincipal).");
                return RedirectToPage();
            }

            // Tenta obter o objeto Utilizador da base de dados usando o ID
            //var user = await _userManager.FindByIdAsync(userId); // Alternativa se GetUserAsync estiver a falhar consistentemente no POST
            var user = await _userManager.GetUserAsync(User); // Vamos tentar com GetUserAsync primeiro, pois é mais direto

            if (user == null)
            {
                // Se GetUserAsync falhar, podemos tentar diretamente pelo context se necessário,
                // mas é estranho GetUserAsync falhar se o utilizador está autenticado e tem ID.
                _logger.LogWarning("OnPostCancelarReservaAsync: _userManager.GetUserAsync(User) retornou null para UserId: {UserId}. Tentando FindByIdAsync.", userId);
                user = await _userManager.FindByIdAsync(userId); // Tenta encontrar pelo ID como fallback
                if (user == null)
                {
                    ErrorMessage = "Os seus dados de utilizador não puderam ser carregados. Ação não permitida.";
                    _logger.LogWarning("OnPostCancelarReservaAsync: Utilizador não encontrado com ID: {UserId} após múltiplas tentativas.", userId);
                    return RedirectToPage();
                }
            }

            // Agora que temos 'user', continuamos como antes
            var reservaParaRemover = await _context.Reservas
                                                .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.UtilizadorIdentityId == user.Id);

            if (reservaParaRemover == null)
            {
                ErrorMessage = "Reserva não encontrada ou não tem permissão para esta ação.";
                _logger.LogWarning("OnPostCancelarReservaAsync: Reserva não encontrada ({IdReserva}) ou sem permissão para o utilizador {UserId} ({UserName}).", idReserva, user.Id, user.UserName);
                return RedirectToPage();
            }

         

            try
            {
                _context.Reservas.Remove(reservaParaRemover);
                await _context.SaveChangesAsync();
                StatusMessage = "Reserva removida com sucesso!";
                _logger.LogInformation("OnPostCancelarReservaAsync: Reserva {IdReserva} REMOVIDA com sucesso pelo utilizador {UserId} ({UserName}).", idReserva, user.Id, user.UserName);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "OnPostCancelarReservaAsync: Erro ao REMOVER a reserva {IdReserva} para o utilizador {UserId} ({UserName}).", idReserva, user.Id, user.UserName);
                ErrorMessage = "Ocorreu um erro ao tentar remover a reserva. Por favor, tente novamente.";
            }

            return RedirectToPage();
        }
    }
}