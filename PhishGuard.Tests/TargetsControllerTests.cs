using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PhishGuard.Tests;

public class TargetsControllerTests
{
    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    private static (AppDbContext context, FakeTenantProvider tenantProvider) CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantProvider = new FakeTenantProvider();
        var context = new AppDbContext(options, tenantProvider);

        return (context, tenantProvider);
    }

    private static Tenant CriarTenant(string cnpj)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = "Empresa Teste",
            Cnpj = cnpj,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task PostAlvo_DeveAssociarCorretamenteAoTenantAtivo()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();

        var tenantA = CriarTenant("11111111000191");
        context.Tenants.Add(tenantA);
        await context.SaveChangesAsync();

        tenantProvider.TenantIdAtivo = tenantA.Id;

        var controller = new TargetsController(context, tenantProvider);

        var novoAlvo = new Target
        {
            Nome = "Fulano de Tal",
            Email = "fulano@teste.com",
            Departamento = "TI"
        };

        // Act
        var resultado = await controller.PostAlvo(novoAlvo);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Assert.Equal(201, createdResult.StatusCode);

        var alvoCriado = Assert.IsType<Target>(createdResult.Value);
        Assert.NotEqual(Guid.Empty, alvoCriado.Id);

        var alvoNoBanco = await context.Targets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == alvoCriado.Id);

        Assert.NotNull(alvoNoBanco);
        Assert.Equal(tenantA.Id, alvoNoBanco!.TenantId);
    }

    [Fact]
    public async Task GetAlvos_DeveRetornarApenasAlvosDoTenantAtivo()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();

        var tenantA = CriarTenant("11111111000191");
        var tenantB = CriarTenant("22222222000172");
        context.Tenants.AddRange(tenantA, tenantB);
        await context.SaveChangesAsync();

        // Alimentando o laboratório com múltiplos registros válidos no Tenant A
        tenantProvider.TenantIdAtivo = tenantA.Id;
        context.Targets.AddRange(
            new Target { Id = Guid.NewGuid(), Nome = "Alvo Valido 1", Email = "alvoa1@teste.com", Departamento = "TI" },
            new Target { Id = Guid.NewGuid(), Nome = "Alvo Valido 2", Email = "alvoa2@teste.com", Departamento = "RH" }
        );
        await context.SaveChangesAsync();

        tenantProvider.TenantIdAtivo = tenantB.Id;
        context.Targets.Add(new Target
        {
            Id = Guid.NewGuid(),
            Nome = "Alvo Tenant B",
            Email = "alvob@teste.com",
            Departamento = "RH"
        });
        await context.SaveChangesAsync();

        // Voltando o contexto para o Tenant A para realizar a consulta
        tenantProvider.TenantIdAtivo = tenantA.Id;
        var controller = new TargetsController(context, tenantProvider);

        // Act
        var resultado = await controller.GetAlvos();

        // Assert
        var alvos = Assert.IsAssignableFrom<IEnumerable<Target>>(resultado.Value).ToList();

        Assert.Equal(2, alvos.Count); // Verifica se os dois registros válidos retornaram
        Assert.All(alvos, a => Assert.Equal(tenantA.Id, a.TenantId));
        Assert.DoesNotContain(alvos, a => a.Nome == "Alvo Tenant B");
    }

    [Theory]
    [InlineData("Alvo A1", "alvoa1teste.com", "TI")] // Sem o @
    [InlineData("Alvo A2", "Nada", "RH")]            // String genérica sem formato
    [InlineData("Alvo A3", "", "Financeiro")]        // E-mail vazio
    public async Task PostAlvo_ComEmailInvalido_DeveRetornarBadRequest(string nome, string emailInvalido, string depto)
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        
        var tenantA = CriarTenant("11111111000191");
        context.Tenants.Add(tenantA);
        await context.SaveChangesAsync();

        tenantProvider.TenantIdAtivo = tenantA.Id;
        var controller = new TargetsController(context, tenantProvider);

        // Força a simulação da validação de modelo do ASP.NET Core para cenários unitários
        if (string.IsNullOrWhiteSpace(emailInvalido) || !emailInvalido.Contains("@"))
        {
            controller.ModelState.AddModelError("Email", "Formato de e-mail inválido.");
        }

        var alvoInvalido = new Target
        {
            Nome = nome,
            Email = emailInvalido,
            Departamento = depto
        };

        // Act
        var resultado = await controller.PostAlvo(alvoInvalido);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task DeleteAlvo_DeOutroTenant_DeveRetornarNotFound()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();

        var tenantA = CriarTenant("11111111000191");
        var tenantB = CriarTenant("22222222000172");
        context.Tenants.AddRange(tenantA, tenantB);
        await context.SaveChangesAsync();

        tenantProvider.TenantIdAtivo = tenantB.Id;
        var alvoDoTenantB = new Target
        {
            Id = Guid.NewGuid(),
            Nome = "Alvo Tenant B",
            Email = "alvob@teste.com",
            Departamento = "RH"
        };
        context.Targets.Add(alvoDoTenantB);
        await context.SaveChangesAsync();

        tenantProvider.TenantIdAtivo = tenantA.Id;
        var controller = new TargetsController(context, tenantProvider);

        // Act
        var resultado = await controller.DeleteAlvo(alvoDoTenantB.Id);

        // Assert
        Assert.IsType<NotFoundResult>(resultado);

        var alvoAindaExiste = await context.Targets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == alvoDoTenantB.Id);
        Assert.NotNull(alvoAindaExiste);
    }
}