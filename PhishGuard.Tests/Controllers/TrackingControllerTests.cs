using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Contracts;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using Xunit;

namespace PhishGuard.Tests.Controllers;

// Suíte do TrackingController — o ÚNICO endpoint [AllowAnonymous] que um alvo (ou atacante)
// toca sem autenticar. Cobre idempotência, contrato de redirect, a garantia LGPD server-side
// (o evento é registrado, a credencial NUNCA) e o gap do targetId forjado (Passo 6).
//
// O tracking roda SEM contexto de tenant (anônimo): o FakeTenantProvider devolve Guid.Empty,
// então o SaveChangesAsync NÃO carimba e o log preserva o TenantId da própria campanha.
// Por isso as asserções sobre logs usam IgnoreQueryFilters (o tenant ativo é Guid.Empty).
public class TrackingControllerTests
{
    private const string BaseUrl = "https://phish.example";

    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    private static AppDbContext NovoContexto() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            new FakeTenantProvider { TenantIdAtivo = Guid.Empty });

    private static TrackingController NovoControlador(AppDbContext ctx, string? ip = "203.0.113.7")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:PublicAppBaseUrl"] = BaseUrl,
            })
            .Build();

        var http = new DefaultHttpContext();
        if (ip != null)
            http.Connection.RemoteIpAddress = IPAddress.Parse(ip);

        return new TrackingController(ctx, config)
        {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
    }

    private static async Task<Campaign> AdicionarCampanhaAsync(AppDbContext ctx, Guid? landingPageId = null)
    {
        var campanha = new Campaign
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            NomeCampanha = "Campanha de Teste",
            Status = "Em Andamento",
            DataInicio = DateTime.UtcNow,
            EmailTemplateId = Guid.NewGuid(),
            LandingPageId = landingPageId ?? Guid.NewGuid(),
            EducationalPageId = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
        };
        ctx.Campaigns.Add(campanha);
        await ctx.SaveChangesAsync();
        return campanha;
    }

    // Alvo REAL pertencente ao tenant informado — necessário desde o hardening do Passo 6
    // (o tracking só registra ação para targetId que exista e seja do mesmo tenant).
    private static async Task<Target> AdicionarAlvoAsync(AppDbContext ctx, Guid tenantId)
    {
        var alvo = new Target
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Nome = "Alvo Real",
            Email = "alvo@empresa.com",
            Departamento = "TI",
        };
        ctx.Targets.Add(alvo);
        await ctx.SaveChangesAsync();
        return alvo;
    }

    private static Task<int> ContarLogs(AppDbContext ctx, Guid campId, Guid tgtId, string acao) =>
        ctx.SimulationsLogs.IgnoreQueryFilters()
            .CountAsync(l => l.CampaignId == campId && l.TargetId == tgtId && l.Acao == acao);

    // 1) Idempotência: dois cliques do mesmo alvo geram UM log (dedup CampaignId+TargetId+Acao).
    [Fact]
    public async Task TrackClick_DuasChamadas_CriaApenasUmLogDeClique()
    {
        await using var ctx = NovoContexto();
        var camp = await AdicionarCampanhaAsync(ctx);
        var alvo = await AdicionarAlvoAsync(ctx, camp.TenantId);
        var tgt = alvo.Id;
        var controller = NovoControlador(ctx);

        await controller.TrackClick(camp.Id, tgt);
        await controller.TrackClick(camp.Id, tgt);

        Assert.Equal(1, await ContarLogs(ctx, camp.Id, tgt, SimulationActions.Clique));
    }

    // 2) Contrato de redirect: leva à landing do React com os IDs na query string.
    [Fact]
    public async Task TrackClick_RedirecionaParaLandingComCampaignETarget()
    {
        await using var ctx = NovoContexto();
        var landingId = Guid.NewGuid();
        var camp = await AdicionarCampanhaAsync(ctx, landingPageId: landingId);
        var alvo = await AdicionarAlvoAsync(ctx, camp.TenantId);
        var tgt = alvo.Id;
        var controller = NovoControlador(ctx);

        var result = await controller.TrackClick(camp.Id, tgt);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal($"{BaseUrl}/landing/{landingId}?c={camp.Id}&t={tgt}", redirect.Url);
    }

    // 3) LGPD (espelho server-side do teste de frontend): a submissão registra só o EVENTO.
    //    Nem o tamanho da senha (o dado "mais sensível" permitido no DTO) é persistido.
    [Fact]
    public async Task TrackSubmit_PersisteApenasOEvento_NuncaOConteudoSensivel()
    {
        await using var ctx = NovoContexto();
        var camp = await AdicionarCampanhaAsync(ctx);
        var alvo = await AdicionarAlvoAsync(ctx, camp.TenantId);
        var tgt = alvo.Id;
        var controller = NovoControlador(ctx);

        // Valor DISTINTO e improvável para caçar qualquer vazamento na persistência.
        var metadata = new CaptureMetadataDto { CamposPreenchidos = true, SenhasCoincidem = true, TamanhoSenha = 987654 };
        var result = await controller.TrackSubmit(camp.Id, tgt, metadata);

        Assert.IsType<OkObjectResult>(result);

        var log = await ctx.SimulationsLogs.IgnoreQueryFilters()
            .SingleAsync(l => l.CampaignId == camp.Id && l.TargetId == tgt);
        Assert.Equal(SimulationActions.Submissao, log.Acao);
        Assert.Equal("203.0.113.7", log.IpOrigem);

        // Prova estrutural: o registro persistido não contém NENHUM traço do DTO —
        // nem as flags, nem o tamanho da senha (a senha em si nunca chega ao servidor).
        var registroSerializado = JsonSerializer.Serialize(log);
        Assert.DoesNotContain("987654", registroSerializado);
    }

    // 3b) Contrato compartilhado (§1.3d): o redirectUrl do submit usa os parâmetros
    //     CANÔNICOS c/t (os mesmos do front), NUNCA o antigo `campaign=`. É o formato
    //     produzido pelo TrackingContract — o mesmo espelhado no módulo TS.
    [Fact]
    public async Task TrackSubmit_RedirectUrl_SegueContratoCanonicoCeT()
    {
        await using var ctx = NovoContexto();
        var camp = await AdicionarCampanhaAsync(ctx);
        var alvo = await AdicionarAlvoAsync(ctx, camp.TenantId);
        var tgt = alvo.Id;
        var controller = NovoControlador(ctx);

        var result = await controller.TrackSubmit(camp.Id, tgt, new CaptureMetadataDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<TrackSubmitResponseDto>(ok.Value);

        // Formato exato do contrato: /educational-feedback?c={camp}&t={tgt}.
        var esperado = TrackingContract.EducationalFeedbackUrl(null, camp.Id.ToString(), tgt.ToString());
        Assert.Equal(esperado, body.RedirectUrl);
        Assert.Equal($"/educational-feedback?c={camp.Id}&t={tgt}", body.RedirectUrl);

        // Regressão do §1.3d: o parâmetro divergente `campaign=` não pode reaparecer.
        Assert.DoesNotContain("campaign=", body.RedirectUrl);
    }

    // 4) Campanha inexistente (ou link expirado) → NotFound, sem estourar exceção.
    [Fact]
    public async Task TrackSubmit_CampanhaInexistente_RetornaNotFound()
    {
        await using var ctx = NovoContexto();
        var controller = NovoControlador(ctx);

        var result = await controller.TrackSubmit(Guid.NewGuid(), Guid.NewGuid(), new CaptureMetadataDto());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TrackComplete_CampanhaInexistente_RetornaNotFound()
    {
        await using var ctx = NovoContexto();
        var controller = NovoControlador(ctx);

        var result = await controller.TrackComplete(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // 5) GAP DE SEGURANÇA (§2 do relatório) — CORRIGIDO no Passo 6: targetId forjado. Um
    //    caller anônimo POSTando submit/{campanhaReal}/{guidArbitrário} NÃO deve injetar log
    //    órfão nem poluir as métricas do tenant — a resposta é NotFound e nada é persistido.
    [Fact]
    public async Task TrackSubmit_TargetForjado_NaoCriaLogOrfao_ERetornaNotFound()
    {
        await using var ctx = NovoContexto();
        var camp = await AdicionarCampanhaAsync(ctx);
        var controller = NovoControlador(ctx);

        // targetId aleatório, que NÃO pertence à campanha nem ao tenant.
        var result = await controller.TrackSubmit(camp.Id, Guid.NewGuid(), new CaptureMetadataDto());

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, await ctx.SimulationsLogs.IgnoreQueryFilters().CountAsync());
    }
}
