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

	[HttpPost("register")]
	public async Task<IActionResult> Registrar(RegisterDto request)
	{
		
		var novoTenant = new Tenant
		{
			Id = Guid.NewGuid(),
			NomeEmpresa = request.NomeEmpresa, 
			Cnpj = request.Cnpj,
			Ativo = true,
			CriadoEm = DateTime.UtcNow
		};

		_context.Tenants.Add(novoTenant);

		// 2. Cria o Administrador vinculado ao ID da empresa rec�m-criada
		var novoAdmin = new Administrador
		{
			Id = Guid.NewGuid(),
			TenantId = novoTenant.Id,  // Resolve o erro de Foreign Key!
			Nome = request.Nome,       // O Nome que voc� definiu no DTO
			Email = request.Email,
			
			// Substitua esta linha pela sua fun��o real de Hash de Senha (ex: BCrypt)
			SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha) 
		};

		_context.Administradores.Add(novoAdmin);

		// 3. Salva os dois de uma vez s� no PostgreSQL
		await _context.SaveChangesAsync();

		return Ok(new { mensagem = "Empresa e conta administrativa criadas com sucesso!" });
	}

	[HttpPost("login")]
	public async Task<ActionResult<string>> Login(LoginDto request)
	{
		var admin = await _context.Administradores
			.FirstOrDefaultAsync(u => u.Email == request.Email);

		if (admin == null) return BadRequest("Usu�rio ou senha inv�lidos.");

		if (!BCrypt.Net.BCrypt.Verify(request.Senha, admin.SenhaHash))
			return BadRequest("Usu�rio ou senha inv�lidos.");

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