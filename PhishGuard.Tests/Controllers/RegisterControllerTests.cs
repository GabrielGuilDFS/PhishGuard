using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;

namespace PhishGuard.Tests.Controllers;

public class RegisterControllerTests
{
    private const string SenhaValida = "SenhaForte@123";

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

    [Fact]
    public async Task Registrar_ComDadosValidos_DeveCriarTenantEAdministradorERetornarOk()
    {
        // Arrange
        var context = CriarContexto();
        var controller = new RegisterController(context);

        var request = new RegisterDto
        {
            NomeEmpresa = "Empresa Teste",
            Cnpj = "12345678000199",
            Nome = "Admin Teste",
            Email = "Admin@Teste.com",
            Password = SenhaValida
        };

        // Act
        var resultado = await controller.Registrar(request);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);

        var tenantSalvo = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Cnpj == "12345678000199");
        Assert.NotNull(tenantSalvo);
        Assert.Equal("Empresa Teste", tenantSalvo!.NomeEmpresa);

        var adminSalvo = await context.Administradores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Email == "admin@teste.com");
        Assert.NotNull(adminSalvo);

        // O e-mail deve ser persistido normalizado (minúsculas)
        Assert.Equal("admin@teste.com", adminSalvo!.Email);
        // O administrador deve estar corretamente vinculado ao tenant criado
        Assert.Equal(tenantSalvo.Id, adminSalvo.TenantId);
    }

    [Theory]
    [InlineData("123", "empresa@teste.com", "Cnpj", "O CNPJ deve conter exatamente 14 dígitos.")]
    [InlineData("12345678000199", "email-invalido", "Email", "O formato do e-mail é inválido.")]
    public async Task Registrar_ComDadosInvalidos_DeveRespeitarModelStateERetornarBadRequest(
        string cnpj, string email, string campoInvalido, string mensagemErro)
    {
        // Arrange
        var context = CriarContexto();
        var controller = new RegisterController(context);

        // Emula o comportamento do pipeline do ASP.NET Core, que popula o
        // ModelState a partir das Data Annotations antes de chegar na action.
        controller.ModelState.AddModelError(campoInvalido, mensagemErro);

        var request = new RegisterDto
        {
            NomeEmpresa = "Empresa Teste",
            Cnpj = cnpj,
            Nome = "Admin Teste",
            Email = email,
            Password = SenhaValida
        };

        // Act
        var resultado = await controller.Registrar(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);

        var erros = Assert.IsType<SerializableError>(badRequest.Value);
        Assert.True(erros.ContainsKey(campoInvalido));

        // Nenhum registro deve ter sido persistido quando o modelo é inválido
        Assert.False(await context.Administradores.IgnoreQueryFilters().AnyAsync());
        Assert.False(await context.Tenants.IgnoreQueryFilters().AnyAsync());
    }

    [Fact]
    public async Task Registrar_ComEmailDuplicado_DeveRetornarBadRequestComMensagemEspecifica()
    {
        // Arrange
        var context = CriarContexto();

        const string emailExistente = "admin@teste.com";
        context.Administradores.Add(new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Nome = "Admin Existente",
            Email = emailExistente,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(SenhaValida)
        });
        await context.SaveChangesAsync();

        var controller = new RegisterController(context);

        var request = new RegisterDto
        {
            NomeEmpresa = "Nova Empresa",
            Cnpj = "99999999000199",
            Nome = "Novo Admin",
            Email = emailExistente,
            Password = "OutraSenha@123"
        };

        // Act
        var resultado = await controller.Registrar(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        var mensagem = Assert.IsType<string>(badRequest.Value);
        Assert.Contains("Este e-mail já está em uso", mensagem);
    }
}
