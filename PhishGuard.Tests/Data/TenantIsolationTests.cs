using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using Xunit;

namespace PhishGuard.Tests.Data;

// Prova o "coração da segurança" do PhishGuard: os Global Query Filters do AppDbContext
// isolam os dados por tenant, e o SaveChangesAsync carimba o TenantId do provider ativo.
//
// Técnica: um FakeTenantProvider MUTÁVEL simula a troca de tenant no mesmo contexto — o
// filtro `x.TenantId == this.TenantIdAtual` é reavaliado a cada consulta, então basta
// virar o TenantIdAtivo para enxergar (ou não) os dados. É o mesmo padrão já usado em
// CampaignsControllerTests.
public class TenantIsolationTests
{
    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    private static AppDbContext CriarContexto(FakeTenantProvider tp)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, tp);
    }

    // Toda entidade multi-tenant (tem TenantId + Global Query Filter no AppDbContext).
    public enum EntidadeMultiTenant
    {
        Target,
        Campaign,
        Template,
        PhishingPage,
        EducationalPage,
        SimulationLog,
    }

    // Adiciona UMA instância válida do tipo pedido, SEM se preocupar com o TenantId:
    // o SaveChangesAsync sobrescreve pelo tenant ativo. Os campos obrigatórios são
    // preenchidos para não depender do provedor (InMemory não valida, mas mantém realista).
    private static void Adicionar(AppDbContext ctx, EntidadeMultiTenant tipo)
    {
        switch (tipo)
        {
            case EntidadeMultiTenant.Target:
                ctx.Targets.Add(new Target { Nome = "Alvo", Email = "alvo@empresa.com", Departamento = "TI" });
                break;
            case EntidadeMultiTenant.Campaign:
                ctx.Campaigns.Add(new Campaign
                {
                    NomeCampanha = "Campanha",
                    Status = "Rascunho",
                    DataInicio = DateTime.UtcNow,
                    EmailTemplateId = Guid.NewGuid(),
                    LandingPageId = Guid.NewGuid(),
                    EducationalPageId = Guid.NewGuid(),
                    CriadoEm = DateTime.UtcNow,
                });
                break;
            case EntidadeMultiTenant.Template:
                ctx.Templates.Add(new Template
                {
                    Nome = "Modelo",
                    Assunto = "Assunto",
                    RemetenteNome = "Remetente",
                    RemetenteEmail = "no-reply@empresa.com",
                    CorpoHtml = "<p>Olá</p>",
                    CriadoEm = DateTime.UtcNow,
                });
                break;
            case EntidadeMultiTenant.PhishingPage:
                ctx.PhishingPages.Add(new PhishingPage { Nome = "Landing", HtmlCaptura = "<form></form>", CriadoEm = DateTime.UtcNow });
                break;
            case EntidadeMultiTenant.EducationalPage:
                ctx.EducationalPages.Add(new EducationalPage { Nome = "Educacional", HtmlEducacional = "<p>Treino</p>", CriadoEm = DateTime.UtcNow });
                break;
            case EntidadeMultiTenant.SimulationLog:
                ctx.SimulationsLogs.Add(new SimulationLog
                {
                    Id = Guid.NewGuid(),
                    CampaignId = Guid.NewGuid(),
                    TargetId = Guid.NewGuid(),
                    Acao = SimulationActions.Clique,
                    DataHora = DateTime.UtcNow,
                    IpOrigem = "127.0.0.1",
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
        }
    }

    // Conta as linhas VISÍVEIS pelo tenant ativo (respeita o Global Query Filter).
    private static Task<int> ContarVisiveis(AppDbContext ctx, EntidadeMultiTenant tipo) => tipo switch
    {
        EntidadeMultiTenant.Target => ctx.Targets.CountAsync(),
        EntidadeMultiTenant.Campaign => ctx.Campaigns.CountAsync(),
        EntidadeMultiTenant.Template => ctx.Templates.CountAsync(),
        EntidadeMultiTenant.PhishingPage => ctx.PhishingPages.CountAsync(),
        EntidadeMultiTenant.EducationalPage => ctx.EducationalPages.CountAsync(),
        EntidadeMultiTenant.SimulationLog => ctx.SimulationsLogs.CountAsync(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };

    // Conta as linhas REAIS no banco, ignorando o filtro (prova que o dado existe/persistiu).
    private static Task<int> ContarIgnorandoFiltros(AppDbContext ctx, EntidadeMultiTenant tipo) => tipo switch
    {
        EntidadeMultiTenant.Target => ctx.Targets.IgnoreQueryFilters().CountAsync(),
        EntidadeMultiTenant.Campaign => ctx.Campaigns.IgnoreQueryFilters().CountAsync(),
        EntidadeMultiTenant.Template => ctx.Templates.IgnoreQueryFilters().CountAsync(),
        EntidadeMultiTenant.PhishingPage => ctx.PhishingPages.IgnoreQueryFilters().CountAsync(),
        EntidadeMultiTenant.EducationalPage => ctx.EducationalPages.IgnoreQueryFilters().CountAsync(),
        EntidadeMultiTenant.SimulationLog => ctx.SimulationsLogs.IgnoreQueryFilters().CountAsync(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };

    // ------------------------------------------------------------------------------------
    // 1) O caso emblemático do plano: dado do tenant A não vaza para o tenant B (Target).
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task GlobalQueryFilter_NaoVazaDadosEntreTenants()
    {
        var tenantA = Guid.NewGuid();
        var tp = new FakeTenantProvider { TenantIdAtivo = tenantA };
        await using var ctx = CriarContexto(tp);

        ctx.Targets.Add(new Target { Nome = "Alvo A", Email = "a@empresa.com", Departamento = "TI" });
        await ctx.SaveChangesAsync();

        tp.TenantIdAtivo = Guid.NewGuid();                                    // "vira" tenant B
        Assert.Empty(await ctx.Targets.ToListAsync());                        // B não vê A
        Assert.Single(await ctx.Targets.IgnoreQueryFilters().ToListAsync()); // o dado existe
    }

    // ------------------------------------------------------------------------------------
    // 2) Isolamento uniforme para TODA entidade multi-tenant (incl. SimulationLog).
    //    Verifica os dois sentidos: B não vê A, e A volta a ver o próprio dado.
    // ------------------------------------------------------------------------------------
    [Theory]
    [InlineData(EntidadeMultiTenant.Target)]
    [InlineData(EntidadeMultiTenant.Campaign)]
    [InlineData(EntidadeMultiTenant.Template)]
    [InlineData(EntidadeMultiTenant.PhishingPage)]
    [InlineData(EntidadeMultiTenant.EducationalPage)]
    [InlineData(EntidadeMultiTenant.SimulationLog)]
    public async Task GlobalQueryFilter_IsolaCadaEntidadePorTenant(EntidadeMultiTenant tipo)
    {
        var tenantA = Guid.NewGuid();
        var tp = new FakeTenantProvider { TenantIdAtivo = tenantA };
        await using var ctx = CriarContexto(tp);

        Adicionar(ctx, tipo);
        await ctx.SaveChangesAsync();                        // carimba tenantA

        // Tenant B: não enxerga nada, mas o dado continua no banco.
        tp.TenantIdAtivo = Guid.NewGuid();
        Assert.Equal(0, await ContarVisiveis(ctx, tipo));
        Assert.Equal(1, await ContarIgnorandoFiltros(ctx, tipo));

        // De volta ao tenant A: enxerga o próprio dado.
        tp.TenantIdAtivo = tenantA;
        Assert.Equal(1, await ContarVisiveis(ctx, tipo));
    }

    // ------------------------------------------------------------------------------------
    // 3) SaveChangesAsync carimba o TenantId do provider — e SOBRESCREVE um valor errado,
    //    impedindo que um insert malicioso/bugado grave em outro tenant.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task SaveChangesAsync_CarimbaTenantIdDoProvider_SobrescrevendoValorErrado()
    {
        var tenantCorreto = Guid.NewGuid();
        var tenantForjado = Guid.NewGuid();
        var tp = new FakeTenantProvider { TenantIdAtivo = tenantCorreto };
        await using var ctx = CriarContexto(tp);

        // Insere de propósito com um TenantId de OUTRO tenant.
        ctx.Targets.Add(new Target { Nome = "X", Email = "x@empresa.com", Departamento = "TI", TenantId = tenantForjado });
        await ctx.SaveChangesAsync();

        var alvo = await ctx.Targets.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(tenantCorreto, alvo.TenantId);   // o carimbo venceu o valor forjado
        Assert.NotEqual(tenantForjado, alvo.TenantId);
    }
}
