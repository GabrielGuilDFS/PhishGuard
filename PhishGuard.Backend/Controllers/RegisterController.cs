using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;

namespace PhishGuard.Backend.Controllers;

[Route("api/auth")]
[ApiController]
public class RegisterController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;
	private readonly ILogger<RegisterController> _logger;

	public RegisterController(
		AppDbContext context,
		IConfiguration? configuration = null,
		ILogger<RegisterController>? logger = null)
	{
		_context = context;
		_configuration = configuration ?? new ConfigurationBuilder().Build();
		_logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RegisterController>.Instance;
	}

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<IActionResult> Registrar(
		RegisterDto request,
		CancellationToken cancellationToken = default)
	{
		var registrationEnabled = _configuration
			.GetValue<bool?>("AppSettings:RegistrationEnabled") ?? true;
		if (!registrationEnabled)
		{
			return StatusCode(StatusCodes.Status403Forbidden, new
			{
				code = "REGISTRATION_DISABLED",
				message = "Novos cadastros estão temporariamente desabilitados."
			});
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var emailNormalizado = request.Email.Trim().ToLowerInvariant();
		var cnpjNormalizado = request.Cnpj.Trim();

		if (await EmailJaEstaEmUsoAsync(emailNormalizado, cancellationToken))
		{
			return Conflito("EMAIL_ALREADY_EXISTS", "Este e-mail já está em uso.");
		}

		if (await CnpjJaEstaEmUsoAsync(cnpjNormalizado, cancellationToken))
		{
			return Conflito("CNPJ_ALREADY_EXISTS", "Este CNPJ já está em uso.");
		}

		var novoTenant = CriarTenant(request, cnpjNormalizado);
		var novoAdmin = CriarAdministrador(request, novoTenant.Id, emailNormalizado);

		_context.Tenants.Add(novoTenant);
		_context.Administradores.Add(novoAdmin);

		try
		{
			// Um único SaveChanges mantém Tenant e Administrador na mesma transação
			// implícita nos provedores relacionais.
			await _context.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException exception) when (ObterViolacaoUnicidade(exception) is { } conflito)
		{
			// Cobre a corrida entre a consulta preventiva e o INSERT no banco.
			_context.ChangeTracker.Clear();
			return Conflito(conflito.Code, conflito.Message);
		}
		catch (DbUpdateException exception)
		{
			_logger.LogError(exception, "Falha ao persistir novo tenant durante o registro.");
			return Problem(
				statusCode: StatusCodes.Status500InternalServerError,
				title: "Não foi possível concluir o cadastro.",
				detail: "Tente novamente em alguns instantes.");
		}

		return Ok(new { mensagem = "Empresa e conta administrativa criadas com sucesso!" });
	}

	private Task<bool> EmailJaEstaEmUsoAsync(string emailNormalizado, CancellationToken cancellationToken) =>
		_context.Administradores
			.IgnoreQueryFilters()
			.AnyAsync(a => a.Email == emailNormalizado, cancellationToken);

	private Task<bool> CnpjJaEstaEmUsoAsync(string cnpj, CancellationToken cancellationToken) =>
		_context.Tenants.AnyAsync(t => t.Cnpj == cnpj, cancellationToken);

	private static Tenant CriarTenant(RegisterDto request, string cnpjNormalizado) => new Tenant
	{
		Id = Guid.NewGuid(),
		NomeEmpresa = request.NomeEmpresa.Trim(),
		Cnpj = cnpjNormalizado,
		Ativo = true,
		CriadoEm = DateTime.UtcNow,
		Plano = PlanoLimites.DeTexto(request.Plano)
	};

	private static Administrador CriarAdministrador(RegisterDto request, Guid tenantId, string emailNormalizado) => new Administrador
	{
		Id = Guid.NewGuid(),
		TenantId = tenantId,
		Nome = request.Nome.Trim(),
		Email = emailNormalizado,
		PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
	};

	private static ConflictObjectResult Conflito(string code, string message) =>
		new(new { code, message });

	private static (string Code, string Message)? ObterViolacaoUnicidade(DbUpdateException exception)
	{
		if (exception.InnerException is not PostgresException
			{ SqlState: PostgresErrorCodes.UniqueViolation } postgresException)
		{
			return null;
		}

		return postgresException.ConstraintName switch
		{
			"ix_administradores_email" => ("EMAIL_ALREADY_EXISTS", "Este e-mail já está em uso."),
			"ix_tenants_cnpj" => ("CNPJ_ALREADY_EXISTS", "Este CNPJ já está em uso."),
			_ => null
		};
	}
}
