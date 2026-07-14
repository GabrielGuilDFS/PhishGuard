using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
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
        private readonly ISmtpCredentialProtector _senhaProtector;

        public SmtpConfigController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            ISmtpCredentialProtector senhaProtector)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _senhaProtector = senhaProtector;
        }

        [HttpGet]
        public async Task<ActionResult<SmtpConfigDto>> Get()
        {
            var config = await _context.SmtpConfigs.FirstOrDefaultAsync();
            if (config == null) return NotFound("Nenhuma configuração de SMTP encontrada para este tenant.");

            // A senha (cifrada) NUNCA é devolvida ao frontend — só um indicador de que
            // existe. Isso evita expor a credencial e evita que o próximo save a
            // sobrescreva com uma máscara enviada de volta pela tela.
            return new SmtpConfigDto
            {
                Host = config.Host,
                Porta = config.Porta,
                Usuario = config.Usuario,
                Senha = "",
                SenhaConfigurada = !string.IsNullOrEmpty(config.Senha)
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

                // Só sobrescreve a senha quando o formulário envia uma nova (o GET
                // devolve senha vazia). Cifra ANTES de persistir (proteção em repouso).
                if (!string.IsNullOrWhiteSpace(dto.Senha))
                {
                    config.Senha = _senhaProtector.Protect(dto.Senha);
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
                    Senha = _senhaProtector.Protect(dto.Senha)
                };

                _context.SmtpConfigs.Add(config);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Configuração de SMTP salva com sucesso." });
        }

        [HttpPost("Testar")]
        public async Task<IActionResult> TestarConexao([FromBody] TestarSmtpDto config)
        {
            var tenantId = _tenantProvider.GetTenantId();
            string host;
            int porta;
            string usuario;
            string senha;
            string emailDestino;

            if (config.TargetId.HasValue)
            {
                var target = await _context.Targets.FirstOrDefaultAsync(t => t.Id == config.TargetId.Value && t.TenantId == tenantId);
                if (target == null) return NotFound("Alvo não encontrado.");

                emailDestino = target.Email;

                var smtpConfig = await _context.SmtpConfigs.FirstOrDefaultAsync(s => s.TenantId == tenantId);
                if (smtpConfig == null) return BadRequest("Configuração SMTP não cadastrada no banco de dados.");

                host = smtpConfig.Host;
                porta = smtpConfig.Porta;
                usuario = smtpConfig.Usuario;
                senha = _senhaProtector.Unprotect(smtpConfig.Senha); // decifra só no uso
            }
            else
            {
                // Teste manual da tela de Configurações. Os campos podem vir do
                // formulário OU, quando o usuário já salvou, do registro persistido:
                // a senha NUNCA é recarregada na tela por segurança (o GET devolve
                // Senha vazia) e ainda é limpa após salvar, então o formulário
                // frequentemente envia a senha em branco mesmo com a config salva.
                // Faz o merge (formulário tem prioridade; cai para o salvo quando
                // vazio) para o teste ser resiliente após salvar/recarregar.
                var smtpSalvo = await _context.SmtpConfigs.FirstOrDefaultAsync(s => s.TenantId == tenantId);

                host = !string.IsNullOrWhiteSpace(config.Host) ? config.Host : (smtpSalvo?.Host ?? "");
                porta = config.Porta > 0 ? config.Porta : (smtpSalvo?.Porta ?? 0);
                usuario = !string.IsNullOrWhiteSpace(config.Usuario) ? config.Usuario : (smtpSalvo?.Usuario ?? "");
                // Senha do formulário (texto puro) tem prioridade; senão, decifra a salva.
                senha = !string.IsNullOrWhiteSpace(config.Senha)
                    ? config.Senha
                    : _senhaProtector.Unprotect(smtpSalvo?.Senha);
                emailDestino = config.EmailDestino ?? "";

                if (string.IsNullOrEmpty(host)) return BadRequest("O campo Host é obrigatório.");
                if (porta <= 0) return BadRequest("A Porta é inválida.");
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
                    return BadRequest("Usuário e Senha são obrigatórios para provedores reais.");
                if (string.IsNullOrEmpty(emailDestino))
                    return BadRequest("O campo EmailDestino é obrigatório.");
            }

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(host, porta, SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(usuario, senha);
                
                // Envia de fato o e-mail de teste
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PhishGuard (Teste)", usuario));
                message.To.Add(new MailboxAddress(emailDestino, emailDestino));
                message.Subject = "Teste de Conexão SMTP - PhishGuard";
                
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"<h3>Teste de Envio</h3><p>Este é um e-mail de teste disparado pelo PhishGuard para confirmar que a conexão com o servidor SMTP (<strong>{host}</strong>) está funcionando perfeitamente.</p>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                return Ok(new { message = $"Conexão e envio de teste realizados com sucesso para {emailDestino}!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Falha no envio de teste SMTP: {ex.Message}");
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
        public Guid? TargetId { get; set; }
    }
}

