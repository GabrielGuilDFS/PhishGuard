using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;
using MimeKit;

namespace PhishGuard.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SmtpConfigController : ControllerBase
    {
        private class PassthroughEmailSecretProtector : IEmailSecretProtector
        {
            private readonly ISmtpCredentialProtector _inner;
            public PassthroughEmailSecretProtector(ISmtpCredentialProtector inner) => _inner = inner;
            public string Protect(string? plaintext) => _inner.Protect(plaintext);
            public string Unprotect(string? stored) => _inner.Unprotect(stored);
            public string ProtectSecret(Guid tenantId, EmailProviderType providerType, EmailSecretType secretType, string? plaintext)
                => _inner.Protect(plaintext);
            public string UnprotectSecret(Guid tenantId, EmailProviderType providerType, EmailSecretType secretType, string? stored)
                => _inner.Unprotect(stored);
        }

        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IEmailSecretProtector _secretProtector;
        private readonly IEmailSenderResolver _senderResolver;
        private readonly IConfiguration _configuration;

        [ActivatorUtilitiesConstructor]
        public SmtpConfigController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            IEmailSecretProtector secretProtector,
            IEmailSenderResolver senderResolver,
            IConfiguration configuration)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _secretProtector = secretProtector;
            _senderResolver = senderResolver;
            _configuration = configuration;
        }

        // Construtor de compatibilidade para testes unitarios legados (internal: visível
        // apenas via InternalsVisibleTo, sem ambiguidade com o ctor principal para a DI).
        internal SmtpConfigController(
            AppDbContext context,
            ITenantProvider tenantProvider,
            ISmtpCredentialProtector senhaProtector,
            ISmtpClientFactory smtpClientFactory,
            IConfiguration configuration)
            : this(context, tenantProvider,
                   senhaProtector is IEmailSecretProtector esp ? esp : new PassthroughEmailSecretProtector(senhaProtector),
                   new EmailSenderResolver(new IEmailSender[]
                   {
                       new SmtpEmailSender(smtpClientFactory, senhaProtector, configuration, Microsoft.Extensions.Logging.Abstractions.NullLogger<SmtpEmailSender>.Instance),
                       new ProviderApiEmailSender(new System.Net.Http.HttpClient(), senhaProtector is IEmailSecretProtector esp2 ? esp2 : new PassthroughEmailSecretProtector(senhaProtector), Microsoft.Extensions.Logging.Abstractions.NullLogger<ProviderApiEmailSender>.Instance)
                   }),
                   configuration)
        {
        }

        [HttpGet("status")]
        [HttpGet("~/api/email-delivery/status")]
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
                    if (config!.ProviderType == EmailProviderType.ProviderApi)
                        _secretProtector.UnprotectSecret(
                            tenantId,
                            EmailProviderType.ProviderApi,
                            EmailSecretType.ApiKey,
                            config.EncryptedApiKey);
                    else
                        _secretProtector.UnprotectSecret(
                            tenantId,
                            EmailProviderType.Smtp,
                            EmailSecretType.SmtpPassword,
                            config.Senha);
                }
                catch (SmtpOperationalException ex)
                {
                    credentialErrorCode = ex.Code;
                }
            }

            return Ok(SmtpOperationalPolicy.ToStatus(config, _configuration, credentialErrorCode));
        }

        [HttpGet]
        [HttpGet("~/api/email-delivery/config")]
        public async Task<ActionResult<SmtpConfigDto>> Get(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized();
            var config = await _context.SmtpConfigs.FirstOrDefaultAsync(
                s => s.TenantId == tenantId,
                cancellationToken);
            if (config == null) return NotFound("Nenhuma configuração de envio de e-mail encontrada para este tenant.");

            return new SmtpConfigDto
            {
                ProviderType = config.ProviderType,
                ApiProvider = config.ApiProvider,
                SenderEmail = config.SenderEmail,
                SenderName = config.SenderName,
                ApiKey = "",
                ApiKeyConfigured = !string.IsNullOrEmpty(config.EncryptedApiKey),
                ApiAccountIdentifier = config.ApiAccountIdentifier,
                ApiRegion = config.ApiRegion,
                Host = config.Host,
                Porta = config.Porta,
                Usuario = config.Usuario,
                Senha = "",
                SenhaConfigurada = !string.IsNullOrEmpty(config.Senha)
            };
        }

        [HttpPut]
        [HttpPut("~/api/email-delivery/config")]
        public async Task<IActionResult> Upsert([FromBody] SmtpConfigDto dto, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Tenant não identificado.");

            if (!Enum.IsDefined(dto.ProviderType))
                return BadRequest(new { code = "EMAIL_PROVIDER_TYPE_INVALID", message = "Selecione um transporte de e-mail válido." });

            if (dto.ProviderType == EmailProviderType.Smtp)
            {
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
            }
            else
            {
                if (!Enum.IsDefined(dto.ApiProvider))
                    return BadRequest(new { code = "API_PROVIDER_INVALID", message = "Selecione um provedor por API suportado." });
                if (dto.ApiKey?.Length > 2048)
                    return BadRequest(new { code = "API_KEY_INVALID", message = "A API Key excede o tamanho permitido." });
                if (dto.ApiKey?.Any(char.IsControl) == true)
                    return BadRequest(new { code = "API_KEY_INVALID", message = "A API Key contém caracteres inválidos." });

                var senderEmail = dto.SenderEmail?.Trim() ?? string.Empty;
                if (!MailboxAddress.TryParse(senderEmail, out var senderMailbox)
                    || !string.Equals(senderMailbox.Address, senderEmail, StringComparison.OrdinalIgnoreCase)
                    || senderEmail.Length > 254)
                    return BadRequest(new { code = "SENDER_EMAIL_INVALID", message = "Informe um e-mail de remetente válido e autorizado no provedor." });

                var senderName = dto.SenderName?.Trim() ?? string.Empty;
                if (senderName.Length > 100 || senderName.Contains('\r') || senderName.Contains('\n'))
                    return BadRequest(new { code = "SENDER_NAME_INVALID", message = "O nome do remetente deve ter no máximo 100 caracteres." });

                if ((dto.ApiAccountIdentifier?.Length ?? 0) > 200)
                    return BadRequest(new { code = "API_ACCOUNT_INVALID", message = "O identificador da conta excede o tamanho permitido." });
                if (dto.ApiAccountIdentifier?.Any(char.IsControl) == true)
                    return BadRequest(new { code = "API_ACCOUNT_INVALID", message = "O identificador da conta contém caracteres inválidos." });

                if (dto.ApiProvider == ApiProviderName.AwsSes)
                {
                    if (string.IsNullOrWhiteSpace(dto.ApiAccountIdentifier))
                        return BadRequest(new { code = "AWS_ACCESS_KEY_ID_REQUIRED", message = "Informe o AWS Access Key ID." });
                    if (!EmailProviderPolicy.IsSupportedAwsRegion(dto.ApiRegion))
                        return BadRequest(new { code = "AWS_REGION_INVALID", message = "Selecione uma região AWS SES suportada." });
                }
                else if (dto.ApiProvider == ApiProviderName.MailtrapSandbox
                    && !EmailProviderPolicy.TryParseMailtrapSandboxId(dto.ApiAccountIdentifier, out _))
                {
                    return BadRequest(new
                    {
                        code = "MAILTRAP_SANDBOX_ID_INVALID",
                        message = "Informe um Sandbox ID numérico válido do Mailtrap."
                    });
                }
            }

            var config = await _context.SmtpConfigs
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

            if (config == null)
            {
                config = new SmtpConfig
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId
                };
                _context.SmtpConfigs.Add(config);
            }

            if (dto.ProviderType == EmailProviderType.Smtp
                && string.IsNullOrWhiteSpace(dto.Senha)
                && string.IsNullOrWhiteSpace(config.Senha))
                return BadRequest(new { code = "SMTP_PASSWORD_REQUIRED", message = "Informe a senha ou senha de aplicativo SMTP." });
            if (dto.ProviderType == EmailProviderType.ProviderApi
                && string.IsNullOrWhiteSpace(dto.ApiKey)
                && (string.IsNullOrWhiteSpace(config.EncryptedApiKey)
                    || config.ProviderType != EmailProviderType.ProviderApi
                    || config.ApiProvider != dto.ApiProvider))
                return BadRequest(new { code = "API_KEY_REQUIRED", message = "Informe a credencial secreta do provedor." });

            config.ProviderType = dto.ProviderType;
            config.ApiProvider = dto.ApiProvider;
            config.SenderEmail = dto.SenderEmail?.Trim() ?? string.Empty;
            config.SenderName = dto.SenderName?.Trim() ?? string.Empty;
            config.ApiAccountIdentifier = dto.ApiAccountIdentifier?.Trim() ?? string.Empty;
            config.ApiRegion = string.IsNullOrWhiteSpace(dto.ApiRegion)
                ? "us-east-1"
                : dto.ApiRegion.Trim().ToLowerInvariant();

            if (dto.ProviderType == EmailProviderType.Smtp)
            {
                config.Host = dto.Host?.Trim() ?? string.Empty;
                config.Porta = dto.Porta;
                config.Usuario = dto.Usuario?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(dto.Senha))
                {
                    config.Senha = _secretProtector.ProtectSecret(
                        tenantId,
                        EmailProviderType.Smtp,
                        EmailSecretType.SmtpPassword,
                        dto.Senha);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.ApiKey))
                {
                    config.EncryptedApiKey = _secretProtector.ProtectSecret(
                        tenantId,
                        EmailProviderType.ProviderApi,
                        EmailSecretType.ApiKey,
                        dto.ApiKey);
                }
            }

            config.UltimoTesteSucesso = null;
            config.UltimoTesteEmUtc = null;
            config.UltimoErroCodigo = null;

            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new
            {
                mensagem = "Configuração de entrega de e-mail salva com sucesso.",
                status = SmtpOperationalPolicy.ToStatus(config, _configuration)
            });
        }

        [HttpPost("Testar")]
        [HttpPost("~/api/email-delivery/test")]
        public async Task<IActionResult> TestarConexao(
            [FromBody] TestarSmtpDto config,
            CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty) return Unauthorized();
            var smtpSalvo = await _context.SmtpConfigs
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

            if (smtpSalvo == null || !SmtpOperationalPolicy.IsConfigured(smtpSalvo))
            {
                return Conflict(new
                {
                    code = SmtpOperationalPolicy.NotConfiguredCode,
                    message = "Salve uma configuração de entrega de e-mail completa antes de executar o teste."
                });
            }

            if (smtpSalvo.ProviderType == EmailProviderType.Smtp && !SmtpOperationalPolicy.IsTransportEnabled(_configuration))
            {
                smtpSalvo.UltimoTesteEmUtc = DateTime.UtcNow;
                smtpSalvo.UltimoTesteSucesso = false;
                smtpSalvo.UltimoErroCodigo = SmtpOperationalPolicy.TransportUnavailableCode;
                await _context.SaveChangesAsync(cancellationToken);

                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    code = SmtpOperationalPolicy.TransportUnavailableCode,
                    message = SmtpOperationalPolicy.GetTransportDisabledReason(_configuration)
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

            if (!MailboxAddress.TryParse(emailDestino, out var destination)
                || !string.Equals(destination.Address, emailDestino, StringComparison.OrdinalIgnoreCase)
                || emailDestino.Length > 254)
                return BadRequest(new { code = "EMAIL_DESTINATION_INVALID", message = "Informe um e-mail de destino válido." });

            var sender = _senderResolver.Resolve(smtpSalvo);
            var testResult = await sender.TestAsync(smtpSalvo, emailDestino, cancellationToken);

            smtpSalvo.UltimoTesteEmUtc = DateTime.UtcNow;
            smtpSalvo.UltimoTesteSucesso = testResult.Success;
            smtpSalvo.UltimoErroCodigo = testResult.ErrorCode;
            await _context.SaveChangesAsync(cancellationToken);

            if (testResult.Success)
            {
                return Ok(new { message = testResult.Message });
            }

            var errorCode = testResult.ErrorCode ?? "TEST_FAILED";
            return StatusCode(
                StatusCodeParaFalha(errorCode),
                new { code = errorCode, message = testResult.Message });
        }

        private static int StatusCodeParaFalha(string code) => code switch
        {
            SmtpOperationalPolicy.CredentialUnreadableCode => StatusCodes.Status409Conflict,
            SmtpOperationalPolicy.AuthenticationFailedCode or "PROVIDER_AUTH_FAILED" =>
                StatusCodes.Status422UnprocessableEntity,
            SmtpOperationalPolicy.ConnectionTimeoutCode => StatusCodes.Status504GatewayTimeout,
            SmtpOperationalPolicy.ConnectionFailedCode or "PROVIDER_API_UNAVAILABLE" =>
                StatusCodes.Status502BadGateway,
            "PROVIDER_RATE_LIMITED" => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
