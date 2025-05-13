using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Aluguer_Salas.Views.Alugar
{
    [Authorize]
    public class AluguerModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AluguerModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Sala Sala { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int salaId { get; set; } // <--- VOLTAR PARA salaId (ou manter se j� estava assim)

        [BindProperty]
        [Required(ErrorMessage = "A data da reserva � obrigat�ria.")]
        [DataType(DataType.Date)]
        public DateTime DataReserva { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "A hora de in�cio � obrigat�ria.")]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicioForm { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A hora de fim � obrigat�ria.")]
        [DataType(DataType.Time)]
        public TimeSpan HoraFimForm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id) // Par�metro do m�todo continua 'id' (corresponde a asp-route-id e @page "{id:int?}")
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID da sala n�o fornecido.";
                return RedirectToPage("/Alugar/Salas");
            }

            // Atribuir o par�metro 'id' da rota � propriedade 'salaId' do PageModel
            this.salaId = id.Value;

            if (_context.Salas == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro de configura��o do sistema: Reposit�rio de salas indispon�vel.");
            }

            // Usar this.salaId para buscar no banco de dados
            var salaParaAlugar = await _context.Salas.FirstOrDefaultAsync(s => s.Id == this.salaId);

            if (salaParaAlugar == null)
            {
                TempData["ErrorMessage"] = $"Sala com ID {this.salaId} n�o encontrada.";
                return NotFound();
            }

            Sala = salaParaAlugar;
            if (DataReserva == DateTime.MinValue) DataReserva = DateTime.Today;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // A propriedade 'salaId' ser� preenchida pelo input hidden <input type="hidden" asp-for="salaId" />
            if (_context.Salas == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro de configura��o: Reposit�rio de salas indispon�vel.");
            }
            // Usar this.salaId para buscar no banco de dados
            Sala = await _context.Salas.FindAsync(this.salaId);
            if (Sala == null)
            {
                TempData["ErrorMessage"] = "Sala n�o encontrada para completar o aluguer. O ID pode n�o ser mais v�lido.";
                return RedirectToPage("/Alugar/Salas");
            }

            if (DataReserva.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(DataReserva), "A data da reserva n�o pode ser no passado.");
            }

            if (HoraFimForm <= HoraInicioForm)
            {
                ModelState.AddModelError(nameof(HoraFimForm), "A hora de fim deve ser posterior � hora de in�cio.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor, corrija os erros no formul�rio.";
                return Page();
            }

            if (_context.Reservas == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro de configura��o do sistema: Reposit�rio de reservas indispon�vel.");
            }

            var utilizadorIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(utilizadorIdentityId))
            {
                return Challenge();
            }

            DateTime dataHoraInicioReserva = DataReserva.Date.Add(HoraInicioForm);
            DateTime dataHoraFimReserva = DataReserva.Date.Add(HoraFimForm);

            // Usar this.salaId na consulta
            bool conflitoExistente = await _context.Reservas
                .AnyAsync(r => r.IdSala == this.salaId &&
                               r.Status != "Cancelada" &&
                               ((dataHoraInicioReserva >= r.HoraInicio && dataHoraInicioReserva < r.HoraFim) ||
                                (dataHoraFimReserva > r.HoraInicio && dataHoraFimReserva <= r.HoraFim) ||
                                (dataHoraInicioReserva <= r.HoraInicio && dataHoraFimReserva >= r.HoraFim)));

            if (conflitoExistente)
            {
                TempData["ErrorMessage"] = $"A sala '{Sala.NomeSala}' j� est� reservada para o per�odo de {dataHoraInicioReserva:dd/MM/yyyy HH:mm} a {dataHoraFimReserva:HH:mm}. Por favor, escolha outro hor�rio.";
                return Page();
            }

            var novaReserva = new Reserva
            {
                IdSala = this.salaId, // Usar this.salaId
                UtilizadorIdentityId = utilizadorIdentityId,
                Data = DataReserva.Date,
                HoraInicio = dataHoraInicioReserva,
                HoraFim = dataHoraFimReserva,
                Status = "Confirmada"
            };

            try
            {
                _context.Reservas.Add(novaReserva);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Sala '{Sala.NomeSala}' alugada com sucesso para {DataReserva:dd/MM/yyyy} das {HoraInicioForm:hh\\:mm} �s {HoraFimForm:hh\\:mm}!";
                return RedirectToPage("/Alugar/Salas");
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao tentar guardar a reserva. Por favor, tente novamente. Detalhe: " + ex.Message;
                return Page();
            }
        }
    }
}