using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using Xunit;

namespace PhishGuard.Tests.Controllers;

public class EducationalPagesControllerTests
{
    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    private static (AppDbContext Context, FakeTenantProvider TenantProvider) CriarContexto()
    {
        var provider = new FakeTenantProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return (new AppDbContext(options, provider), provider);
    }

    [Fact]
    public async Task PutPage_DeveRestaurarNomeSemTrocarIdOuConteudo()
    {
        var (context, provider) = CriarContexto();
        provider.TenantIdAtivo = Guid.NewGuid();
        var id = Guid.NewGuid();
        const string html = "<div data-feedback-training=\"mercadoliv\"></div>";
        context.EducationalPages.Add(new EducationalPage
        {
            Id = id,
            Nome = "Cenário descontinuado",
            HtmlEducacional = html,
            CriadoEm = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var controller = new EducationalPagesController(context, provider);
        var resultado = await controller.PutPage(id, new PageInputDto
        {
            Nome = "Treinamento Interativo — Mercado Liv",
            ConteudoHtml = html,
        });

        Assert.IsType<NoContentResult>(resultado);
        var pagina = await context.EducationalPages.SingleAsync(p => p.Id == id);
        Assert.Equal(id, pagina.Id);
        Assert.Equal("Treinamento Interativo — Mercado Liv", pagina.Nome);
        Assert.Equal(html, pagina.HtmlEducacional);
    }

    [Fact]
    public async Task PutPage_NaoDeveAlterarPaginaDeOutroTenant()
    {
        var (context, provider) = CriarContexto();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        provider.TenantIdAtivo = tenantA;
        var pagina = new EducationalPage
        {
            Id = Guid.NewGuid(),
            Nome = "Cenário descontinuado",
            HtmlEducacional = "<div data-feedback-training=\"mercadoliv\"></div>",
            CriadoEm = DateTime.UtcNow,
        };
        context.EducationalPages.Add(pagina);
        await context.SaveChangesAsync();

        provider.TenantIdAtivo = tenantB;
        var controller = new EducationalPagesController(context, provider);
        var resultado = await controller.PutPage(pagina.Id, new PageInputDto
        {
            Nome = "Treinamento Interativo — Mercado Liv",
            ConteudoHtml = pagina.HtmlEducacional,
        });

        Assert.IsType<NotFoundResult>(resultado);
        var persistida = await context.EducationalPages
            .IgnoreQueryFilters()
            .SingleAsync(p => p.Id == pagina.Id);
        Assert.Equal("Cenário descontinuado", persistida.Nome);
        Assert.Equal(tenantA, persistida.TenantId);
    }
}
