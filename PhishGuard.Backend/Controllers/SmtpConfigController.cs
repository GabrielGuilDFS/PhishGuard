using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using System.Net;
using System.Net.Mail;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SmtpConfigController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public SmtpConfigController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        public async Task<ActionResult<SmtpConfigDto>> Get()
        {
            var config = await _context.SmtpConfigs.FirstOrDefaultAsync();
            if (config == null) return NotFound("Nenhuma configuração de SMTP encontrada para este tenant.");

            return new SmtpConfigDto
            {
                Host = config.Host,
                Porta = config.Porta,
                Usuario = config.Usuario,
                Senha = ""
            };
        }

        [HttpPut]
        public async Task<IActionResult> Upsert([FromBody] SmtpConfigDto dto)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Tenant não identificado.");

            var config = await _context.SmtpConfigs.FirstOrDefaultAsync();

            if (config != null)
            {
                config.Host = dto.Host;
                config.Porta = dto.Porta;
                config.Usuario = dto.Usuario;

                if (!string.IsNullOrWhiteSpace(dto.Senha))
                {
                    config.Senha = dto.Senha;
                }
            }
            else
            {
                config = new SmtpConfig
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Host = dto.Host,
                    Porta = dto.Porta,
                    Usuario = dto.Usuario,
                    Senha = dto.Senha
                };

                _context.SmtpConfigs.Add(config);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Configuração de SMTP salva com sucesso." });
        }

        [HttpPost("Testar")]
        public async Task<IActionResult> Testar([FromBody] TesteSmtpDto dto)
        {
            var config = await _context.SmtpConfigs.FirstOrDefaultAsync();

            if (config == null)
                return BadRequest("Configure o SMTP primeiro antes de testar.");

            try
            {
                using var smtpClient = new SmtpClient(config.Host, config.Porta)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(config.Usuario, config.Senha)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress("teste@phishguard.com", "Sistema PhishGuard"), 
                    Subject = "PhishGuard - Teste de Conexão SMTP",
                    Body = "Se você está lendo isso, o motor de disparo do PhishGuard está funcionando perfeitamente!"
                };

                message.To.Add(dto.EmailDestino);

                await smtpClient.SendMailAsync(message);

                return Ok("E-mail de teste enviado com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Falha ao enviar: {ex.Message}");
            }
        }
    }
}

