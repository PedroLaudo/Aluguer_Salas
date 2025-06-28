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
    public class RequisicaoMaterialApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequisicaoMaterialApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as requisições de material do sistema.
        /// </summary>
        /// <returns></returns>

        // GET: api/RequisicaoMaterialApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequisicaoMaterial>>> GetRequisicoesMaterial()
        {
            return await _context.RequisicoesMaterial.ToListAsync();
        }


        /// <summary>
        /// Obtém uma requisição de material específica pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/RequisicaoMaterialApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequisicaoMaterial>> GetRequisicaoMaterial(int id)
        {
            var requisicao = await _context.RequisicoesMaterial.FindAsync(id);

            if (requisicao == null)
            {
                return NotFound();
            }

            return requisicao;
        }

        /// <summary>
        /// Atualiza uma requisição de material existente.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        // PUT: api/RequisicaoMaterialApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequisicaoMaterial(int id, RequisicaoMaterial requisicao)
        {
            if (id != requisicao.Id)
            {
                return BadRequest();
            }

            _context.Entry(requisicao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequisicaoMaterialExists(id))
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
        /// Cria uma nova requisição de material.
        /// </summary>
        /// <param name="requisicao"></param>
        /// <returns></returns>
        // POST: api/RequisicaoMaterialApi
        [HttpPost]
        public async Task<ActionResult<RequisicaoMaterial>> PostRequisicaoMaterial(RequisicaoMaterial requisicao)
        {
            _context.RequisicoesMaterial.Add(requisicao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRequisicaoMaterial), new { id = requisicao.Id }, requisicao);
        }


        /// <summary>
        /// Remove uma requisição de material específica pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/RequisicaoMaterialApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequisicaoMaterial(int id)
        {
            var requisicao = await _context.RequisicoesMaterial.FindAsync(id);
            if (requisicao == null)
            {
                return NotFound();
            }

            _context.RequisicoesMaterial.Remove(requisicao);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        /// Verifica se uma requisição de material existe pelo ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool RequisicaoMaterialExists(int id)
        {
            return _context.RequisicoesMaterial.Any(e => e.Id == id);
        }
    }
}
