using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace PhishGuard.Backend.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class AlvosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlvosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alvo>>> GetAlvos()
        {
            return await _context.Alvos.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Alvo>> PostAlvo(Alvo alvo)
        {
            _context.Alvos.Add(alvo);
            await _context.SaveChangesAsync();
            return Ok(alvo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlvo(Guid id)
        {
            var alvo = await _context.Alvos.FirstOrDefaultAsync(a => a.Id == id);
            
            if (alvo == null) return NotFound();

            _context.Alvos.Remove(alvo);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlvo(Guid id, Alvo alvo)
        {
            if (id != alvo.Id) return BadRequest();

            var alvoExistente = await _context.Alvos.FirstOrDefaultAsync(a => a.Id == id);
            
            if (alvoExistente == null) return NotFound();

            alvoExistente.Nome = alvo.Nome;
            alvoExistente.Email = alvo.Email;
            alvoExistente.Departamento = alvo.Departamento;

            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}