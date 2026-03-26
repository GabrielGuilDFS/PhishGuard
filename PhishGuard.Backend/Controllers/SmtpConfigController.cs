using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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
        public async Task<IActionResult> TestarConexao([FromBody] TestarSmtpDto config)
        {
            if (string.IsNullOrEmpty(config.Host)) return BadRequest("O campo Host é obrigatório.");
            if (config.Porta <= 0) return BadRequest("A Porta é inválida.");
            if (string.IsNullOrEmpty(config.Usuario) || string.IsNullOrEmpty(config.Senha)) 
                return BadRequest("Usuário e Senha são obrigatórios para provedores reais.");

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(config.Host, config.Porta, SecureSocketOptions.StartTls);
                
                // Tenta logar de verdade no Gmail/Outlook
                await client.AuthenticateAsync(config.Usuario, config.Senha);
                
                await client.DisconnectAsync(true);
                
                return Ok(new { message = "Conexão estabelecida com segurança!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Falha na autenticação SMTP: {ex.Message}");
            }
        }
    }
    public class TestarSmtpDto
    {
        public string? Host { get; set; }
        public int Porta { get; set; }
        public string? Usuario { get; set; }
        public string? Senha { get; set; }
        public string? EmailDestino { get; set; }
    }
}

