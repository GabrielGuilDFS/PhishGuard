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
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace PhishGuard.Backend.Controllers;

[Route("api/auth")]
[ApiController]
public class LoginController : ControllerBase
{
	private const string REFRESH_COOKIE = "phishguard_refresh";
	// Lockout de conta: após MAX_TENTATIVAS falhas consecutivas, a conta fica
	// bloqueada por DURACAO_BLOQUEIO. Complementa (não substitui) o rate-limiting
	// por IP: o IP-limit trava o brute force distribuído/rápido; o lockout protege
	// uma conta específica mesmo que o atacante rode a partir de vários IPs.
	private const int MAX_TENTATIVAS = 5;
	private static readonly TimeSpan DURACAO_BLOQUEIO = TimeSpan.FromMinutes(15);

	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;
	private readonly TimeProvider _timeProvider;

	public LoginController(AppDbContext context, IConfiguration configuration, TimeProvider timeProvider)
	{
		_context = context;
		_configuration = configuration;
		_timeProvider = timeProvider;
	}

	[AllowAnonymous]
	[EnableRateLimiting("login")]
	[HttpPost("login")]
	public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
	{
		var emailNormalizado = request.Email.Trim().ToLowerInvariant();

		// Login não tem contexto de tenant (o JWT ainda não existe) → IgnoreQueryFilters.
		var admin = await (
			from administrator in _context.Administradores.IgnoreQueryFilters()
			join tenant in _context.Tenants on administrator.TenantId equals tenant.Id
			where administrator.Email == emailNormalizado && tenant.Ativo
			select administrator)
			.FirstOrDefaultAsync();

		// Resposta genérica para conta inexistente: não vaza se o e-mail existe.
		if (admin == null) return BadRequest("Usuário ou senha inválidos.");

		// Conta em lockout ativo: rejeita antes de verificar a senha. A resposta é
		// IDÊNTICA (mensagem + status 400) à do caminho de senha inválida — de propósito:
		// uma resposta diferente aqui revelaria que o e-mail existe (enumeração de contas).
		// O rate-limit por IP (429) é a barreira visível de força-bruta; o lockout age
		// silenciosamente, sem dar pista ao atacante.
		var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
		if (admin.BloqueioFim.HasValue && admin.BloqueioFim.Value > utcNow)
			return BadRequest("Usuário ou senha inválidos.");

		if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
		{
			admin.AcessoFalhasContador++;

			if (admin.AcessoFalhasContador >= MAX_TENTATIVAS)
			{
				admin.BloqueioFim = utcNow.Add(DURACAO_BLOQUEIO);
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

		var refreshToken = CreateRefreshToken();
		var sessionDays = _configuration.GetValue<int?>("AppSettings:RefreshSessionDays") ?? 7;
		if (sessionDays is < 1 or > 30)
			throw new InvalidOperationException("A validade da sessão deve estar entre 1 e 30 dias.");

		var session = new AuthSession
		{
			Id = Guid.NewGuid(),
			TenantId = admin.TenantId,
			AdministratorId = admin.Id,
			RefreshTokenHash = HashRefreshToken(refreshToken),
			CreatedAtUtc = utcNow,
			ExpiresAtUtc = utcNow.AddDays(sessionDays)
		};
		_context.AuthSessions.Add(session);
		await _context.SaveChangesAsync();
		SetRefreshCookie(refreshToken, session.ExpiresAtUtc);

		return Ok(CriarToken(admin, session.Id));
	}

	[AllowAnonymous]
	[HttpPost("refresh")]
	public async Task<ActionResult<AuthResponseDto>> Refresh(CancellationToken cancellationToken = default)
	{
		if (!Request.Cookies.TryGetValue(REFRESH_COOKIE, out var refreshToken)
			|| string.IsNullOrWhiteSpace(refreshToken))
			return Unauthorized();

		var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
		var tokenHash = HashRefreshToken(refreshToken);
		var session = await _context.AuthSessions
			.IgnoreQueryFilters()
			.SingleOrDefaultAsync(item => item.RefreshTokenHash == tokenHash, cancellationToken);

		if (session is null || session.RevokedAtUtc.HasValue || session.ExpiresAtUtc <= nowUtc)
		{
			ClearRefreshCookie();
			return Unauthorized();
		}

		var admin = await (
			from administrator in _context.Administradores.IgnoreQueryFilters()
			join tenant in _context.Tenants on administrator.TenantId equals tenant.Id
			where administrator.Id == session.AdministratorId
				&& administrator.TenantId == session.TenantId
				&& tenant.Id == session.TenantId
				&& tenant.Ativo
			select administrator)
			.SingleOrDefaultAsync(cancellationToken);

		if (admin is null)
		{
			session.RevokedAtUtc = nowUtc;
			await _context.SaveChangesAsync(cancellationToken);
			ClearRefreshCookie();
			return Unauthorized();
		}

		var rotatedRefreshToken = CreateRefreshToken();
		session.RefreshTokenHash = HashRefreshToken(rotatedRefreshToken);
		session.LastRotatedAtUtc = nowUtc;
		await _context.SaveChangesAsync(cancellationToken);
		SetRefreshCookie(rotatedRefreshToken, session.ExpiresAtUtc);

		return Ok(CriarToken(admin, session.Id));
	}

	[AllowAnonymous]
	[HttpPost("logout")]
	public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
	{
		if (Request.Cookies.TryGetValue(REFRESH_COOKIE, out var refreshToken)
			&& !string.IsNullOrWhiteSpace(refreshToken))
		{
			var tokenHash = HashRefreshToken(refreshToken);
			var session = await _context.AuthSessions
				.IgnoreQueryFilters()
				.SingleOrDefaultAsync(item => item.RefreshTokenHash == tokenHash, cancellationToken);
			if (session is not null && !session.RevokedAtUtc.HasValue)
			{
				session.RevokedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
				await _context.SaveChangesAsync(cancellationToken);
			}
		}

		ClearRefreshCookie();
		return NoContent();
	}

	private AuthResponseDto CriarToken(Administrador admin, Guid sessionId)
	{
		var now = _timeProvider.GetUtcNow();
		var issuer = _configuration["AppSettings:JwtIssuer"]
			?? throw new InvalidOperationException("Emissor do JWT não configurado.");
		var audience = _configuration["AppSettings:JwtAudience"]
			?? throw new InvalidOperationException("Audiência do JWT não configurada.");
		var accessTokenMinutes = _configuration.GetValue<int?>("AppSettings:AccessTokenMinutes") ?? 60;
		if (accessTokenMinutes is < 5 or > 1440)
			throw new InvalidOperationException("A validade do access token deve estar entre 5 e 1440 minutos.");

		List<Claim> claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
			new Claim(ClaimTypes.Name, admin.Nome),
			new Claim(ClaimTypes.Email, admin.Email),
			new Claim("tenant_id", admin.TenantId.ToString()),
			new Claim("sid", sessionId.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
			_configuration.GetSection("AppSettings:Token").Value!));

		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			notBefore: now.UtcDateTime,
			expires: now.AddMinutes(accessTokenMinutes).UtcDateTime,
			signingCredentials: creds
		);

		return new AuthResponseDto
		{
			AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
			ExpiresAtUtc = token.ValidTo
		};
	}

	private static string CreateRefreshToken() =>
		WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

	private static string HashRefreshToken(string refreshToken) =>
		Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

	private void SetRefreshCookie(string refreshToken, DateTime expiresAtUtc)
	{
		var crossSite = _configuration.GetValue<bool>("AppSettings:CrossSiteRefreshCookie");
		Response.Cookies.Append(REFRESH_COOKIE, refreshToken, new CookieOptions
		{
			HttpOnly = true,
			Secure = crossSite || Request.IsHttps,
			SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Strict,
			Path = "/api/auth",
			Expires = new DateTimeOffset(expiresAtUtc, TimeSpan.Zero),
			IsEssential = true
		});
	}

	private void ClearRefreshCookie()
	{
		var crossSite = _configuration.GetValue<bool>("AppSettings:CrossSiteRefreshCookie");
		Response.Cookies.Delete(REFRESH_COOKIE, new CookieOptions
		{
			HttpOnly = true,
			Secure = crossSite || Request.IsHttps,
			SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Strict,
			Path = "/api/auth"
		});
	}
}
