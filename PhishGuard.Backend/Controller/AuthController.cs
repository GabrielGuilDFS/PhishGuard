using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using PhishGuard.Backend.Data;  
using PhishGuard.Backend.Models;
using PhishGuard.Backend.DTOs;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;

	public AuthController(AppDbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	[HttpPost("registrar")]
	public async Task<ActionResult<Administrador>> Registrar(RegisterDto request)
	{
		if (await _context.Administradores.AnyAsync(u => u.Email == request.Email))
			return BadRequest("Usuário já existe.");

		string senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

		var admin = new Administrador
		{
			Nome = request.Nome,
			Email = request.Email,
			SenhaHash = senhaHash
		};

		_context.Administradores.Add(admin);
		await _context.SaveChangesAsync();

		return Ok("Administrador registrado com sucesso!");
	}

	[HttpPost("login")]
	public async Task<ActionResult<string>> Login(LoginDto request)
	{
		var admin = await _context.Administradores
			.FirstOrDefaultAsync(u => u.Email == request.Email);

		if (admin == null) return BadRequest("Usuário ou senha inválidos.");

		if (!BCrypt.Net.BCrypt.Verify(request.Senha, admin.SenhaHash))
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
			new Claim(ClaimTypes.Email, admin.Email)
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