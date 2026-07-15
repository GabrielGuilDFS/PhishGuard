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
        var (controller, _, email) = await CriarControllerComAdminEContextoAsync();
        return (controller, email);
    }

    private static async Task<(LoginController controller, AppDbContext context, string email)> CriarControllerComAdminEContextoAsync()
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
        return (controller, context, email);
    }

    private static async Task<Administrador> RecarregarAdminAsync(AppDbContext context, string email)
    {
        return await context.Administradores.IgnoreQueryFilters().SingleAsync(a => a.Email == email);
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

    [Fact]
    public async Task Login_ComSenhaInvalida_IncrementaContadorDeFalhas()
    {
        var (controller, context, email) = await CriarControllerComAdminEContextoAsync();

        await controller.Login(new LoginDto { Email = email, Password = "SenhaErrada@000" });

        var admin = await RecarregarAdminAsync(context, email);
        Assert.Equal(1, admin.AcessoFalhasContador);
        Assert.Null(admin.BloqueioFim);
    }

    [Fact]
    public async Task Login_Apos5FalhasConsecutivas_BloqueiaAConta()
    {
        var (controller, context, email) = await CriarControllerComAdminEContextoAsync();

        for (var i = 0; i < 5; i++)
            await controller.Login(new LoginDto { Email = email, Password = "SenhaErrada@000" });

        var admin = await RecarregarAdminAsync(context, email);
        Assert.NotNull(admin.BloqueioFim);
        Assert.True(admin.BloqueioFim > DateTime.UtcNow);
        Assert.Equal(0, admin.AcessoFalhasContador); // contador reinicia ao bloquear
    }

    [Fact]
    public async Task Login_ComContaBloqueada_RejeitaComRespostaGenerica_MesmoComSenhaCorreta()
    {
        var (controller, _, email) = await CriarControllerComAdminEContextoAsync();

        // Dispara o lockout com 5 falhas.
        for (var i = 0; i < 5; i++)
            await controller.Login(new LoginDto { Email = email, Password = "SenhaErrada@000" });

        // 6ª tentativa com a senha CORRETA deve ser barrada pelo lockout — mas com a MESMA
        // resposta genérica (400 + "Usuário ou senha inválidos.") do caminho de senha
        // errada, para NÃO revelar que a conta existe/está bloqueada (anti-enumeração).
        var resultado = await controller.Login(new LoginDto { Email = email, Password = SenhaValida });

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal("Usuário ou senha inválidos.", badRequest.Value);
        // Confirma que NÃO houve emissão de token, apesar da senha correta.
        Assert.IsNotType<OkObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Login_BemSucedido_ZeraContadorDeFalhas()
    {
        var (controller, context, email) = await CriarControllerComAdminEContextoAsync();

        // 3 falhas (abaixo do limite), depois login válido.
        for (var i = 0; i < 3; i++)
            await controller.Login(new LoginDto { Email = email, Password = "SenhaErrada@000" });

        await controller.Login(new LoginDto { Email = email, Password = SenhaValida });

        var admin = await RecarregarAdminAsync(context, email);
        Assert.Equal(0, admin.AcessoFalhasContador);
        Assert.Null(admin.BloqueioFim);
    }
}
