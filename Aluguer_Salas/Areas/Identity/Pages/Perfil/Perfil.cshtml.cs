using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Aluguer_Salas.Models; // Certifique-se que este namespace cont�m Utilizador e Reserva
using Aluguer_Salas.Data;   // Certifique-se que este namespace cont�m ApplicationDbContext
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
                ErrorMessage = $"N�o foi poss�vel carregar os dados do utilizador (ID: {currentUserId ?? "n�o dispon�vel"}).";
                _logger.LogWarning("OnGetAsync: Utilizador n�o encontrado com ID: {UserId}", currentUserId ?? "n�o dispon�vel");
                return RedirectToPage("/Index"); // Considere uma p�gina de erro mais espec�fica
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
                ErrorMessage = "N�o foi poss�vel identificar o utilizador. A��o n�o permitida.";
                _logger.LogWarning("OnPostCancelarReservaAsync: Tentativa de a��o por utilizador n�o identificado (ID n�o encontrado no ClaimsPrincipal).");
                return RedirectToPage();
            }

            // Tenta obter o objeto Utilizador da base de dados usando o ID
            //var user = await _userManager.FindByIdAsync(userId); // Alternativa se GetUserAsync estiver a falhar consistentemente no POST
            var user = await _userManager.GetUserAsync(User); // Vamos tentar com GetUserAsync primeiro, pois � mais direto

            if (user == null)
            {
                // Se GetUserAsync falhar, podemos tentar diretamente pelo context se necess�rio,
                // mas � estranho GetUserAsync falhar se o utilizador est� autenticado e tem ID.
                _logger.LogWarning("OnPostCancelarReservaAsync: _userManager.GetUserAsync(User) retornou null para UserId: {UserId}. Tentando FindByIdAsync.", userId);
                user = await _userManager.FindByIdAsync(userId); // Tenta encontrar pelo ID como fallback
                if (user == null)
                {
                    ErrorMessage = "Os seus dados de utilizador n�o puderam ser carregados. A��o n�o permitida.";
                    _logger.LogWarning("OnPostCancelarReservaAsync: Utilizador n�o encontrado com ID: {UserId} ap�s m�ltiplas tentativas.", userId);
                    return RedirectToPage();
                }
            }

            // Agora que temos 'user', continuamos como antes
            var reservaParaRemover = await _context.Reservas
                                                .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.UtilizadorIdentityId == user.Id);

            if (reservaParaRemover == null)
            {
                ErrorMessage = "Reserva n�o encontrada ou n�o tem permiss�o para esta a��o.";
                _logger.LogWarning("OnPostCancelarReservaAsync: Reserva n�o encontrada ({IdReserva}) ou sem permiss�o para o utilizador {UserId} ({UserName}).", idReserva, user.Id, user.UserName);
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