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
    public class UtenteApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UtenteApiController(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Lista todos os utentes do sistema.
        /// </summary>
        /// <returns></returns>
        // GET: api/UtenteApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utente>>> GetUtentes()
        {
            return await _context.Utentes.ToListAsync();
        }

        /// <summary>
        /// Obtém um utente específico pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/UtenteApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Utente>> GetUtente(int id)
        {
            var utente = await _context.Utentes.FindAsync(id);

            if (utente == null)
            {
                return NotFound();
            }

            return utente;
        }


        /// <summary>
        /// Atualiza um utente existente.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="utente"></param>
        /// <returns></returns>
        // PUT: api/UtenteApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtente(int id, Utente utente)
        {
            if (id != utente.Id)
            {
                return BadRequest();
            }

            _context.Entry(utente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtenteExists(id))
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
        /// Cria um novo utente no sistema.
        /// </summary>
        /// <param name="utente"></param>
        /// <returns></returns>
        // POST: api/UtenteApi
        [HttpPost]
        public async Task<ActionResult<Utente>> PostUtente(Utente utente)
        {
            _context.Utentes.Add(utente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUtente), new { id = utente.Id }, utente);
        }


        /// <summary>
        /// Remove um utente do sistema pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/UtenteApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtente(int id)
        {
            var utente = await _context.Utentes.FindAsync(id);
            if (utente == null)
            {
                return NotFound();
            }

            _context.Utentes.Remove(utente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se um utente existe pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UtenteExists(int id)
        {
            return _context.Utentes.Any(e => e.Id == id);
        }
    }
}
