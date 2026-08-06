using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using System.Net;
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
        private readonly ISmtpClientFactory _smtpClientFactory;
        private readonly IConfiguration _configuration;

        public SmtpConfigController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            ISmtpCredentialProtector senhaProtector,
            ISmtpClientFactory smtpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _senhaProtector = senhaProtector;
            _smtpClientFactory = smtpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("status")]
        public async Task<ActionResult<SmtpStatusDto>> GetStatus(CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized();

            var config = await _context.SmtpConfigs
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
            string? credentialErrorCode = null;
            if (SmtpOperationalPolicy.IsConfigured(config))
            {
                try
                {
                    _senhaProtector.Unprotect(config!.Senha);
                }
                catch (SmtpOperationalException ex)
                {
                    credentialErrorCode = ex.Code;
                }
            }

            return Ok(SmtpOperationalPolicy.ToStatus(config, _configuration, credentialErrorCode));
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
        public async Task<IActionResult> Upsert([FromBody] SmtpConfigDto dto, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Tenant não identificado.");

            var host = dto.Host?.Trim() ?? string.Empty;
            var usuario = dto.Usuario?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(host))
                return BadRequest(new { code = "SMTP_HOST_REQUIRED", message = "Informe o host SMTP." });
            if (host.Length > 100)
                return BadRequest(new { code = "SMTP_HOST_INVALID", message = "O host SMTP deve ter no máximo 100 caracteres." });
            if (dto.Porta is <= 0 or > 65535)
                return BadRequest(new { code = "SMTP_PORT_INVALID", message = "Informe uma porta SMTP válida (1-65535)." });
            if (string.IsNullOrWhiteSpace(usuario))
                return BadRequest(new { code = "SMTP_USER_REQUIRED", message = "Informe o usuário/e-mail SMTP." });
            if (usuario.Length > 150)
                return BadRequest(new { code = "SMTP_USER_INVALID", message = "O usuário SMTP deve ter no máximo 150 caracteres." });
            if (dto.Senha?.Length > 512)
                return BadRequest(new { code = "SMTP_PASSWORD_INVALID", message = "A senha SMTP excede o tamanho permitido." });

            var config = await _context.SmtpConfigs
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

            if (config == null && string.IsNullOrWhiteSpace(dto.Senha))
                return BadRequest(new { code = "SMTP_PASSWORD_REQUIRED", message = "Informe a senha ou senha de aplicativo SMTP." });

            if (config != null)
            {
                config.Host = host;
                config.Porta = dto.Porta;
                config.Usuario = usuario;

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
                    Host = host,
                    Porta = dto.Porta,
                    Usuario = usuario,
                    Senha = _senhaProtector.Protect(dto.Senha)
                };

                _context.SmtpConfigs.Add(config);
            }

            config.UltimoTesteSucesso = null;
            config.UltimoTesteEmUtc = null;
            config.UltimoErroCodigo = null;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new
            {
                mensagem = "Configuração de SMTP salva com sucesso.",
                status = SmtpOperationalPolicy.ToStatus(config, _configuration)
            });
        }

        [HttpPost("Testar")]
        public async Task<IActionResult> TestarConexao(
            [FromBody] TestarSmtpDto config,
            CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized();
            var smtpSalvo = await _context.SmtpConfigs
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

            if (!SmtpOperationalPolicy.IsTransportEnabled(_configuration))
            {
                if (smtpSalvo != null)
                {
                    smtpSalvo.UltimoTesteEmUtc = DateTime.UtcNow;
                    smtpSalvo.UltimoTesteSucesso = false;
                    smtpSalvo.UltimoErroCodigo = SmtpOperationalPolicy.TransportUnavailableCode;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    code = SmtpOperationalPolicy.TransportUnavailableCode,
                    message = SmtpOperationalPolicy.GetTransportDisabledReason(_configuration)
                });
            }

            if (!SmtpOperationalPolicy.IsConfigured(smtpSalvo))
            {
                return Conflict(new
                {
                    code = SmtpOperationalPolicy.NotConfiguredCode,
                    message = "Salve uma configuração SMTP completa antes de executar o teste."
                });
            }

            string emailDestino;

            if (config.TargetId.HasValue)
            {
                var target = await _context.Targets.FirstOrDefaultAsync(
                    t => t.Id == config.TargetId.Value && t.TenantId == tenantId,
                    cancellationToken);
                if (target == null) return NotFound("Alvo não encontrado.");

                emailDestino = target.Email;
            }
            else
            {
                emailDestino = config.EmailDestino?.Trim() ?? string.Empty;
            }

            if (!MailboxAddress.TryParse(emailDestino, out var destinatario))
                return BadRequest(new { code = "SMTP_DESTINATION_INVALID", message = "Informe um e-mail de destino válido." });
            if (!MailboxAddress.TryParse(smtpSalvo!.Usuario, out var remetente))
                return BadRequest(new { code = "SMTP_SENDER_INVALID", message = "O usuário SMTP salvo não é um endereço de e-mail válido." });

            try
            {
                // O teste usa exclusivamente o registro persistido. Assim ele comprova que
                // o save e a recuperação da credencial funcionam, em vez de testar valores
                // ainda presentes apenas no formulário do navegador.
                var senha = _senhaProtector.Unprotect(smtpSalvo.Senha);
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));
                using var client = _smtpClientFactory.Create();
                var socketOption = Enum.TryParse<SecureSocketOptions>(
                    _configuration["AppSettings:SmtpSecureSocketOptions"], true, out var configuredOption)
                    ? configuredOption
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(smtpSalvo.Host, smtpSalvo.Porta, socketOption, timeoutCts.Token);
                
                await client.AuthenticateAsync(smtpSalvo.Usuario, senha, timeoutCts.Token);
                
                // Envia de fato o e-mail de teste
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("PhishGuard (Teste)", remetente.Address));
                message.To.Add(destinatario);
                message.Subject = "Teste de Conexão SMTP - PhishGuard";
                
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Teste de envio do PhishGuard. A conexão com o servidor SMTP {smtpSalvo.Host} foi concluída com sucesso."
                };
                message.Body = bodyBuilder.ToMessageBody();

                await client.SendAsync(message, timeoutCts.Token);
                await client.DisconnectAsync(true, timeoutCts.Token);

                if (smtpSalvo != null)
                {
                    smtpSalvo.UltimoTesteEmUtc = DateTime.UtcNow;
                    smtpSalvo.UltimoTesteSucesso = true;
                    smtpSalvo.UltimoErroCodigo = null;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                
                return Ok(new { message = $"Conexão e envio de teste realizados com sucesso para {emailDestino}!" });
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return await RegistrarFalhaTesteAsync(
                    smtpSalvo,
                    SmtpOperationalPolicy.ConnectionTimeoutCode,
                    "A conexão SMTP excedeu 15 segundos. Verifique host, porta e as restrições de rede da hospedagem.",
                    cancellationToken,
                    StatusCodes.Status504GatewayTimeout);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var falha = SmtpOperationalPolicy.Classify(ex);
                return await RegistrarFalhaTesteAsync(
                    smtpSalvo,
                    falha.Code,
                    falha.Message,
                    cancellationToken,
                    StatusCodeParaFalha(falha.Code));
            }
        }

        private async Task<IActionResult> RegistrarFalhaTesteAsync(
            SmtpConfig? config,
            string code,
            string message,
            CancellationToken cancellationToken,
            int statusCode = StatusCodes.Status400BadRequest)
        {
            if (config != null)
            {
                config.UltimoTesteEmUtc = DateTime.UtcNow;
                config.UltimoTesteSucesso = false;
                config.UltimoErroCodigo = code;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return StatusCode(statusCode, new { code, message });
        }

        private static int StatusCodeParaFalha(string code) => code switch
        {
            SmtpOperationalPolicy.CredentialUnreadableCode => StatusCodes.Status409Conflict,
            SmtpOperationalPolicy.AuthenticationFailedCode => StatusCodes.Status422UnprocessableEntity,
            SmtpOperationalPolicy.ConnectionTimeoutCode => StatusCodes.Status504GatewayTimeout,
            SmtpOperationalPolicy.ConnectionFailedCode => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public class TestarSmtpDto
    {
        public string? EmailDestino { get; set; }
        public Guid? TargetId { get; set; }
    }
}

