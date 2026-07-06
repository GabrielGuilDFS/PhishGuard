using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;

namespace PhishGuard.Backend.Controllers;

[Route("api/auth")]
[ApiController]
public class LoginController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;

	public LoginController(AppDbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	[AllowAnonymous]
	[HttpPost("login")]
	public async Task<ActionResult<string>> Login(LoginDto request)
	{
		var emailNormalizado = request.Email.ToLower();

		var admin = await _context.Administradores
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Email == emailNormalizado);

		if (admin == null) return BadRequest("Usuário ou senha inválidos.");

		if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
			return BadRequest("Usuário ou senha inválidos.");

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
