using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservaApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservaApiController(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Lista todas as reservas do sistema.
        /// </summary>
        /// <returns></returns>
        // GET: api/ReservaApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            return await _context.Reservas.ToListAsync();
        }

        /// <summary>
        /// Obtém uma reserva específica pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/ReservaApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound();
            }

            return reserva;
        }

        /// <summary>
        /// Atualiza uma reserva existente.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reserva"></param>
        /// <returns></returns>
        // PUT: api/ReservaApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return BadRequest();
            }

            _context.Entry(reserva).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /// <summary>
        /// Cria uma nova reserva.
        /// </summary>
        /// <param name="reserva"></param>
        /// <returns></returns>
        // POST: api/ReservaApi
        [HttpPost]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.IdReserva }, reserva);
        }

        /// <summary>
        /// Remove uma reserva existente pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ReservaApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        /// Verifica se uma reserva existe pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }
    }
}
