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
// TÉCNICA (Passo 1 — o melhor das duas propostas): SQLite IN-MEMORY em vez do provedor
// InMemory do EF Core. É um banco relacional REAL — impõe FKs, unique e a tradução de
// query — sem o peso do Docker/Testcontainers. Assim os filtros e o comportamento
// relacional são exercitados com fidelidade. A troca de tenant usa um provider MUTÁVEL
// (ver SqliteTenantHarness): o filtro `x.TenantId == this.TenantIdAtual` é reavaliado a
// cada consulta, então basta virar o Id para enxergar (ou não) os dados.
public class TenantIsolationTests
{
    // Cnpj é UNIQUE no schema — sob SQLite isso é IMPOSTO. Gera um valor de 14 dígitos.
    private static string Cnpj() => Guid.NewGuid().ToString("N").Substring(0, 14);

    // Semeia um GRAFO COMPLETO e VÁLIDO (satisfazendo todas as FKs físicas) sob o tenant
    // ativo: Tenant → Template/PhishingPage/EducationalPage/Target → Campaign → SimulationLog.
    // Devolve os ids úteis para as asserções de leitura por id.
    private static async Task<(Guid campaignId, Guid targetId)> SemearGrafoAsync(AppDbContext db, Guid tenantId)
    {
        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            NomeEmpresa = "Empresa",
            Cnpj = Cnpj(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            Plano = PlanoTenant.Bronze,
        });

        var template = new Template
        {
            Id = Guid.NewGuid(),
            Nome = "Modelo",
            Assunto = "Assunto",
            RemetenteNome = "Remetente",
            RemetenteEmail = "no-reply@empresa.com",
            CorpoHtml = "<p>Olá</p>",
            CriadoEm = DateTime.UtcNow,
        };
        var phishing = new PhishingPage { Id = Guid.NewGuid(), Nome = "Landing", HtmlCaptura = "<form></form>", CriadoEm = DateTime.UtcNow };
        var edu = new EducationalPage { Id = Guid.NewGuid(), Nome = "Educacional", HtmlEducacional = "<p>Treino</p>", CriadoEm = DateTime.UtcNow };
        var target = new Target { Id = Guid.NewGuid(), Nome = "Alvo", Email = "alvo@empresa.com", Departamento = "TI" };

        db.Templates.Add(template);
        db.PhishingPages.Add(phishing);
        db.EducationalPages.Add(edu);
        db.Targets.Add(target);
        await db.SaveChangesAsync(); // carimba TenantId = tenantId em todos (Tenant não tem TenantId)

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            NomeCampanha = "Campanha",
            Status = CampaignStatus.Rascunho,
            DataInicio = DateTime.UtcNow,
            EmailTemplateId = template.Id,
            LandingPageId = phishing.Id,
            EducationalPageId = edu.Id,
            CriadoEm = DateTime.UtcNow,
        };
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();

        db.SimulationsLogs.Add(new SimulationLog
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            TargetId = target.Id,
            Acao = SimulationActions.Clique,
            DataHora = DateTime.UtcNow,
            IpOrigem = "127.0.0.1",
        });
        await db.SaveChangesAsync();

        return (campaign.Id, target.Id);
    }

    // ------------------------------------------------------------------------------------
    // 1) Isolamento uniforme: NENHUMA entidade multi-tenant vaza para outro tenant, e o
    //    dado continua no banco (prova que é o filtro, não ausência de dado). Volta ao A
    //    e enxerga tudo de novo.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task GlobalQueryFilter_IsolaTodasAsEntidadesPorTenant()
    {
        var tenantA = Guid.NewGuid();
        using var h = new SqliteTenantHarness(tenantA);
        await SemearGrafoAsync(h.Db, tenantA);

        // Vira para o Tenant B: não enxerga NADA de A.
        h.UsarTenant(Guid.NewGuid());
        Assert.Empty(await h.Db.Targets.ToListAsync());
        Assert.Empty(await h.Db.Campaigns.ToListAsync());
        Assert.Empty(await h.Db.Templates.ToListAsync());
        Assert.Empty(await h.Db.PhishingPages.ToListAsync());
        Assert.Empty(await h.Db.EducationalPages.ToListAsync());
        Assert.Empty(await h.Db.SimulationsLogs.ToListAsync());

        // Mas o dado EXISTE (o que muda é só a visibilidade via filtro).
        Assert.Equal(1, await h.Db.Targets.IgnoreQueryFilters().CountAsync());
        Assert.Equal(1, await h.Db.Campaigns.IgnoreQueryFilters().CountAsync());
        Assert.Equal(1, await h.Db.SimulationsLogs.IgnoreQueryFilters().CountAsync());

        // De volta ao Tenant A: enxerga o próprio grafo.
        h.UsarTenant(tenantA);
        Assert.Single(await h.Db.Targets.ToListAsync());
        Assert.Single(await h.Db.Campaigns.ToListAsync());
        Assert.Single(await h.Db.SimulationsLogs.ToListAsync());
    }

    // ------------------------------------------------------------------------------------
    // 2) Anti-IDOR: consulta EXPLÍCITA por Id de um recurso do Tenant A, feita sob o Tenant
    //    B, retorna null — o filtro global barra a leitura direcionada por id (o padrão
    //    correto é FirstOrDefault(x => x.Id == id), nunca FindAsync que contorna o filtro).
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task ConsultaPorId_DeRecursoDeOutroTenant_RetornaNull()
    {
        var tenantA = Guid.NewGuid();
        using var h = new SqliteTenantHarness(tenantA);
        var (campaignId, targetId) = await SemearGrafoAsync(h.Db, tenantA);

        // Sob o Tenant B, mesmo sabendo o Id exato, não alcança o recurso do A.
        h.UsarTenant(Guid.NewGuid());
        Assert.Null(await h.Db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId));
        Assert.Null(await h.Db.Targets.FirstOrDefaultAsync(t => t.Id == targetId));

        // O mesmo id, sob o Tenant A, resolve normalmente.
        h.UsarTenant(tenantA);
        Assert.NotNull(await h.Db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId));
    }

    // ------------------------------------------------------------------------------------
    // 3) SaveChangesAsync carimba o TenantId do provider — e SOBRESCREVE um valor forjado,
    //    impedindo um insert malicioso/bugado de gravar em outro tenant.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task SaveChangesAsync_CarimbaTenantIdDoProvider_SobrescrevendoValorForjado()
    {
        var tenantCorreto = Guid.NewGuid();
        var tenantForjado = Guid.NewGuid();
        using var h = new SqliteTenantHarness(tenantCorreto);

        // Só o tenant CORRETO existe no banco. Target tem FK física TenantId → Tenants
        // (via a navegação Tenant.Alvos), imposta pelo SQLite.
        h.Db.Tenants.Add(new Tenant { Id = tenantCorreto, NomeEmpresa = "OK", Cnpj = Cnpj(), Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze });
        await h.Db.SaveChangesAsync();

        // Insere de propósito com o TenantId de OUTRO tenant (que NÃO existe na tabela).
        h.Db.Targets.Add(new Target { Id = Guid.NewGuid(), Nome = "X", Email = "x@empresa.com", Departamento = "TI", TenantId = tenantForjado });

        // Se o carimbo NÃO reescrevesse para o correto, o insert tentaria gravar o forjado
        // e a FK falharia (não há tal tenant). O sucesso já prova que o carimbo agiu antes
        // do banco; a asserção confirma o valor final.
        await h.Db.SaveChangesAsync();

        var alvo = await h.Db.Targets.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(tenantCorreto, alvo.TenantId);   // o carimbo venceu o valor forjado
        Assert.NotEqual(tenantForjado, alvo.TenantId);
    }

    // ------------------------------------------------------------------------------------
    // 4) FIDELIDADE RELACIONAL (o que o provedor InMemory NÃO pegava): sob SQLite as FKs
    //    são IMPOSTAS. Uma campanha que referencia um Template inexistente é rejeitada pelo
    //    banco — exatamente o tipo de integridade que os testes antigos deixavam passar.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task Sqlite_ImpoeFK_CampanhaComTemplateInexistente_Falha()
    {
        var tenantId = Guid.NewGuid();
        using var h = new SqliteTenantHarness(tenantId);

        // Tenant existe, mas Template/Landing/Educativa apontam para ids que não existem.
        h.Db.Tenants.Add(new Tenant { Id = tenantId, NomeEmpresa = "E", Cnpj = Cnpj(), Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze });
        await h.Db.SaveChangesAsync();

        h.Db.Campaigns.Add(new Campaign
        {
            Id = Guid.NewGuid(),
            NomeCampanha = "Órfã",
            Status = CampaignStatus.Rascunho,
            DataInicio = DateTime.UtcNow,
            EmailTemplateId = Guid.NewGuid(),
            LandingPageId = Guid.NewGuid(),
            EducationalPageId = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => h.Db.SaveChangesAsync());
    }
}
