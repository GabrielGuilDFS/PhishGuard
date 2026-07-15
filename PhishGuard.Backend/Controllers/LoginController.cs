using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;

namespace PhishGuard.Backend.Controllers;

[Route("api/auth")]
[ApiController]
public class LoginController : ControllerBase
{
	// Lockout de conta: após MAX_TENTATIVAS falhas consecutivas, a conta fica
	// bloqueada por DURACAO_BLOQUEIO. Complementa (não substitui) o rate-limiting
	// por IP: o IP-limit trava o brute force distribuído/rápido; o lockout protege
	// uma conta específica mesmo que o atacante rode a partir de vários IPs.
	private const int MAX_TENTATIVAS = 5;
	private static readonly TimeSpan DURACAO_BLOQUEIO = TimeSpan.FromMinutes(15);

	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;

	public LoginController(AppDbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	[AllowAnonymous]
	[EnableRateLimiting("login")]
	[HttpPost("login")]
	public async Task<ActionResult<string>> Login(LoginDto request)
	{
		var emailNormalizado = request.Email.ToLower();

		// Login não tem contexto de tenant (o JWT ainda não existe) → IgnoreQueryFilters.
		var admin = await _context.Administradores
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Email == emailNormalizado);

		// Resposta genérica para conta inexistente: não vaza se o e-mail existe.
		if (admin == null) return BadRequest("Usuário ou senha inválidos.");

		// Conta em lockout ativo: rejeita antes de verificar a senha. A resposta é
		// IDÊNTICA (mensagem + status 400) à do caminho de senha inválida — de propósito:
		// uma resposta diferente aqui revelaria que o e-mail existe (enumeração de contas).
		// O rate-limit por IP (429) é a barreira visível de força-bruta; o lockout age
		// silenciosamente, sem dar pista ao atacante.
		if (admin.BloqueioFim.HasValue && admin.BloqueioFim.Value > DateTime.UtcNow)
			return BadRequest("Usuário ou senha inválidos.");

		if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
		{
			admin.AcessoFalhasContador++;

			if (admin.AcessoFalhasContador >= MAX_TENTATIVAS)
			{
				admin.BloqueioFim = DateTime.UtcNow.Add(DURACAO_BLOQUEIO);
				admin.AcessoFalhasContador = 0; // reinicia a contagem para o próximo ciclo
			}

			await _context.SaveChangesAsync();
			return BadRequest("Usuário ou senha inválidos.");
		}

		// Sucesso: limpa qualquer resíduo de falhas/bloqueio (evita SaveChanges à toa).
		if (admin.AcessoFalhasContador != 0 || admin.BloqueioFim != null)
		{
			admin.AcessoFalhasContador = 0;
			admin.BloqueioFim = null;
			await _context.SaveChangesAsync();
		}

		string token = CriarToken(admin);
		return Ok(token);
	}

	private string CriarToken(Administrador admin)
	{
		List<Claim> claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
			new Claim(ClaimTypes.Name, admin.Nome),
			new Claim(ClaimTypes.Email, admin.Email),
			new Claim("tenant_id", admin.TenantId.ToString())
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
			_configuration.GetSection("AppSettings:Token").Value!));

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

		var token = new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddDays(1),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
