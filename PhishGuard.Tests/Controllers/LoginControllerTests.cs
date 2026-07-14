using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;

namespace PhishGuard.Tests.Controllers;

public class LoginControllerTests
{
    private const string SenhaValida = "SenhaForte@123";
    private const string ChaveTokenTeste = "chave-secreta-somente-para-testes-com-tamanho-suficiente-para-hmacsha512";

    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
        public Guid GetCurrentTenantId() => Guid.Empty;
    }

    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, new FakeTenantProvider());
    }

    private static IConfiguration CriarConfiguracao()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Token"] = ChaveTokenTeste
            })
            .Build();
    }

    private static async Task<(LoginController controller, string email)> CriarControllerComAdminAsync()
    {
        var context = CriarContexto();
        var configuration = CriarConfiguracao();

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = "Empresa Teste",
            Cnpj = "12345678900010",
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var email = "admin@teste.com";
        var admin = new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Nome = "Admin Teste",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(SenhaValida)
        };

        context.Tenants.Add(tenant);
        context.Administradores.Add(admin);
        await context.SaveChangesAsync();

        var controller = new LoginController(context, configuration);
        return (controller, email);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarTokenJwtOk()
    {
        var (controller, email) = await CriarControllerComAdminAsync();

        var resultado = await controller.Login(new LoginDto { Email = email, Password = SenhaValida });

        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(200, okResult.StatusCode);

        var token = Assert.IsType<string>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_DeveRetornarBadRequest()
    {
        var (controller, email) = await CriarControllerComAdminAsync();

        var resultado = await controller.Login(new LoginDto { Email = email, Password = "SenhaErrada@000" });

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_DeveRetornarBadRequest()
    {
        var (controller, _) = await CriarControllerComAdminAsync();

        var resultado = await controller.Login(new LoginDto { Email = "naoexiste@teste.com", Password = SenhaValida });

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
