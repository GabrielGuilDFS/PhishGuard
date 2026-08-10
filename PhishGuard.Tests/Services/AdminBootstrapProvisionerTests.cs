using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Services;

public sealed class AdminBootstrapProvisionerTests
{
    private sealed class EmptyTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
        public Guid GetCurrentTenantId() => Guid.Empty;
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new EmptyTenantProvider());
    }

    private static IConfiguration Configuration(
        string email = "RRAdmin@gmail.com",
        string password = "SenhaBootstrap@123") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BootstrapAdmin:Enabled"] = "true",
                ["BootstrapAdmin:TenantName"] = "Mercado Mendonça",
                ["BootstrapAdmin:Cnpj"] = "49.689.568/9586-12",
                ["BootstrapAdmin:Name"] = "Ricardo Richard Almeida",
                ["BootstrapAdmin:Email"] = email,
                ["BootstrapAdmin:Password"] = password,
                ["BootstrapAdmin:Plan"] = "Bronze"
            })
            .Build();

    private static AdminBootstrapProvisioner CreateProvisioner(
        AppDbContext context,
        IConfiguration configuration) =>
        new(context, configuration, NullLogger<AdminBootstrapProvisioner>.Instance);

    [Fact]
    public async Task ProvisionAsync_CriaTenantEAdminComDadosNormalizadosEHashBcrypt()
    {
        await using var context = CreateContext();
        var provisioner = CreateProvisioner(context, Configuration());

        await provisioner.ProvisionAsync();

        var tenant = await context.Tenants.IgnoreQueryFilters().SingleAsync();
        var admin = await context.Administradores.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("Mercado Mendonça", tenant.NomeEmpresa);
        Assert.Equal("49689568958612", tenant.Cnpj);
        Assert.Equal(PlanoTenant.Bronze, tenant.Plano);
        Assert.Equal("rradmin@gmail.com", admin.Email);
        Assert.Equal(tenant.Id, admin.TenantId);
        Assert.NotEqual("SenhaBootstrap@123", admin.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("SenhaBootstrap@123", admin.PasswordHash));
    }

    [Fact]
    public async Task ProvisionAsync_ExecutadoNovamente_NaoDuplicaNemRedefineSenha()
    {
        await using var context = CreateContext();
        await CreateProvisioner(context, Configuration()).ProvisionAsync();
        var originalHash = (await context.Administradores.IgnoreQueryFilters().SingleAsync()).PasswordHash;

        await CreateProvisioner(context, Configuration(password: "OutraSenha123*")).ProvisionAsync();

        Assert.Equal(1, await context.Tenants.IgnoreQueryFilters().CountAsync());
        Assert.Equal(1, await context.Administradores.IgnoreQueryFilters().CountAsync());
        Assert.Equal(originalHash, (await context.Administradores.IgnoreQueryFilters().SingleAsync()).PasswordHash);
    }

    [Fact]
    public async Task ProvisionAsync_EmailExistenteEmOutroTenant_FalhaSemCriarNovoTenant()
    {
        await using var context = CreateContext();
        var existingTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = "Outro Tenant",
            Cnpj = "11111111000191",
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
        context.Tenants.Add(existingTenant);
        context.Administradores.Add(new Administrador
        {
            Id = Guid.NewGuid(),
            TenantId = existingTenant.Id,
            Nome = "Outro Admin",
            Email = "rradmin@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SenhaExistente123*")
        });
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateProvisioner(context, Configuration()).ProvisionAsync());

        Assert.Contains("conflito", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await context.Tenants.IgnoreQueryFilters().CountAsync());
        Assert.Equal(1, await context.Administradores.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task ProvisionAsync_Desabilitado_NaoAcessaNemAlteraBanco()
    {
        await using var context = CreateContext();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BootstrapAdmin:Enabled"] = "false"
            })
            .Build();

        await CreateProvisioner(context, configuration).ProvisionAsync();

        Assert.False(await context.Tenants.IgnoreQueryFilters().AnyAsync());
        Assert.False(await context.Administradores.IgnoreQueryFilters().AnyAsync());
    }
}
