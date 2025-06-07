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

        // GET: api/RequisicaoMaterialApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequisicaoMaterial>>> GetRequisicoesMaterial()
        {
            return await _context.RequisicoesMaterial.ToListAsync();
        }

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

        // POST: api/RequisicaoMaterialApi
        [HttpPost]
        public async Task<ActionResult<RequisicaoMaterial>> PostRequisicaoMaterial(RequisicaoMaterial requisicao)
        {
            _context.RequisicoesMaterial.Add(requisicao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRequisicaoMaterial), new { id = requisicao.Id }, requisicao);
        }

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

        private bool RequisicaoMaterialExists(int id)
        {
            return _context.RequisicoesMaterial.Any(e => e.Id == id);
        }
    }
}
