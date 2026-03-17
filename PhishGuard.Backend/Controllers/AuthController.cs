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

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<IActionResult> Registrar(RegisterDto request)
	{
		var emailNormalizado = request.Email.ToLower();

		var emailJaUsado = await _context.Administradores
			.IgnoreQueryFilters()
			.AnyAsync(a => a.Email == emailNormalizado);

		if (emailJaUsado)
		{
			return BadRequest("Este e-mail j· est· em uso");
		}
		
		var novoTenant = new Tenant
		{
			Id = Guid.NewGuid(),
			NomeEmpresa = request.NomeEmpresa, 
			Cnpj = request.Cnpj,
			Ativo = true,
			CriadoEm = DateTime.UtcNow
		};

		_context.Tenants.Add(novoTenant);

		// 2. Cria o Administrador vinculado ao ID da empresa recùm-criada
		var novoAdmin = new Administrador
		{
			Id = Guid.NewGuid(),
			TenantId = novoTenant.Id,  // Resolve o erro de Foreign Key!
			Nome = request.Nome,       // O Nome que vocù definiu no DTO
			Email = emailNormalizado,
			
			// Substitua esta linha pela sua funùùo real de Hash de Senha (ex: BCrypt)
			SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha) 
		};

		_context.Administradores.Add(novoAdmin);

		// 3. Salva os dois de uma vez sù no PostgreSQL
		await _context.SaveChangesAsync();

		return Ok(new { mensagem = "Empresa e conta administrativa criadas com sucesso!" });
	}

	[AllowAnonymous]
	[HttpPost("login")]
	public async Task<ActionResult<string>> Login(LoginDto request)
	{
		var emailNormalizado = request.Email.ToLower();

		var admin = await _context.Administradores
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Email == emailNormalizado);

		if (admin == null) return BadRequest("Usuùrio ou senha invùlidos.");

		if (!BCrypt.Net.BCrypt.Verify(request.Senha, admin.SenhaHash))
			return BadRequest("Usuùrio ou senha invùlidos.");

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