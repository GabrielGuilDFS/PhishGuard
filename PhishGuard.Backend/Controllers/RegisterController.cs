using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;

namespace PhishGuard.Backend.Controllers;

[Route("api/auth")]
[ApiController]
public class RegisterController : ControllerBase
{
	private readonly AppDbContext _context;

	public RegisterController(AppDbContext context)
	{
		_context = context;
	}

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<IActionResult> Registrar(RegisterDto request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var emailNormalizado = request.Email.ToLower();

		if (await EmailJaEstaEmUsoAsync(emailNormalizado))
		{
			return BadRequest("Este e-mail já está em uso");
		}

		var novoTenant = CriarTenant(request);
		var novoAdmin = CriarAdministrador(request, novoTenant.Id, emailNormalizado);

		_context.Tenants.Add(novoTenant);
		_context.Administradores.Add(novoAdmin);

		await _context.SaveChangesAsync();

		return Ok(new { mensagem = "Empresa e conta administrativa criadas com sucesso!" });
	}

	private Task<bool> EmailJaEstaEmUsoAsync(string emailNormalizado) =>
		_context.Administradores
			.IgnoreQueryFilters()
			.AnyAsync(a => a.Email == emailNormalizado);

	private static Tenant CriarTenant(RegisterDto request) => new Tenant
	{
		Id = Guid.NewGuid(),
		NomeEmpresa = request.NomeEmpresa,
		Cnpj = request.Cnpj,
		Ativo = true,
		CriadoEm = DateTime.UtcNow
	};

	private static Administrador CriarAdministrador(RegisterDto request, Guid tenantId, string emailNormalizado) => new Administrador
	{
		Id = Guid.NewGuid(),
		TenantId = tenantId,
		Nome = request.Nome,
		Email = emailNormalizado,
		PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
	};
}
