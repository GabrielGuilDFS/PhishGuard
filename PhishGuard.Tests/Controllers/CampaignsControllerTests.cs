using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhishGuard.Backend.BackgroundServices;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PhishGuard.Tests.Controllers;

// Regras de transição de estado das campanhas (Rascunho → Agendada → Processando → Em Andamento):
//  - Ativar com DataInicio no FUTURO  → Agendada (o worker dispara no horário).
//  - Ativar com DataInicio no PASSADO → Processando (CLAIM; o worker envia async, sem
//    disparo bloqueante na thread HTTP).
//  - O worker processa "Agendada" já no horário E "Processando" (retomada); ignora Rascunho.
public class CampaignsControllerTests
{
    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    // Substitui o disparo real (SMTP) nos testes: registra a chamada e aplica a mesma
    // transição de status que o serviço real (→ Em Andamento).
    private sealed class FakeDispatchService : ICampaignDispatchService
    {
        public int Chamadas { get; private set; }
        public Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            Chamadas++;
            campaign.Status = CampaignStatus.EmAndamento;
            return Task.CompletedTask;
        }
    }

    // Dispatcher que SEMPRE falha com uma exceção dada — usado para exercitar a
    // classificação de falha (log robusto) do worker sem SMTP real.
    private sealed class ThrowingDispatchService : ICampaignDispatchService
    {
        private readonly Exception _erro;
        public ThrowingDispatchService(Exception erro) => _erro = erro;
        public Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default) => throw _erro;
    }

    // Logger que captura as mensagens FORMATADAS, para asserir a categoria de falha logada.
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Mensagens { get; } = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Mensagens.Add(formatter(state, exception));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    // Escopo mínimo para o worker: devolve o mesmo AppDbContext e o dispatcher fake.
    private sealed class SingleScopeFactory : IServiceScopeFactory, IServiceScope, IServiceProvider
    {
        private readonly AppDbContext _context;
        private readonly ICampaignDispatchService _dispatcher;
        public SingleScopeFactory(AppDbContext context, ICampaignDispatchService dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }
        public IServiceScope CreateScope() => this;
        public IServiceProvider ServiceProvider => this;
        public void Dispose() { }
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(AppDbContext)) return _context;
            if (serviceType == typeof(ICampaignDispatchService)) return _dispatcher;
            return null;
        }
    }

    private static (AppDbContext context, FakeTenantProvider tenantProvider) CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantProvider = new FakeTenantProvider();
        return (new AppDbContext(options, tenantProvider), tenantProvider);
    }

    // Cria uma campanha em Rascunho (com 1 alvo e 1 template) no tenant informado.
    private static async Task<Campaign> SemearCampanhaRascunhoAsync(
        AppDbContext context, FakeTenantProvider tenantProvider, Guid tenantId, DateTime dataInicio)
    {
        tenantProvider.TenantIdAtivo = tenantId;

        var template = new Template
        {
            Id = Guid.NewGuid(),
            Nome = "Isca",
            Assunto = "Assunto",
            RemetenteNome = "Remetente",
            RemetenteEmail = "remetente@teste.com",
            CorpoHtml = "hbomax-redefinicao-senha"
        };
        var alvo = new Target
        {
            Id = Guid.NewGuid(),
            Nome = "Alvo",
            Email = "alvo@teste.com",
            Departamento = "TI"
        };
        var campanha = new Campaign
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NomeCampanha = "Campanha Teste",
            Status = CampaignStatus.Rascunho,
            DataInicio = dataInicio,
            EmailTemplateId = template.Id,
            LandingPageId = Guid.NewGuid(),
            EducationalPageId = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            Template = template,
            Targets = new List<Target> { alvo }
        };

        context.Templates.Add(template);
        context.Targets.Add(alvo);
        context.Campaigns.Add(campanha);
        await context.SaveChangesAsync();

        return campanha;
    }

    // Semeia os recursos referenciados por uma campanha (isca/página falsa/educativa/alvo)
    // no tenant informado. Requer tenantProvider já apontando para o tenant (o SaveChanges
    // carimba TenantId nas entidades novas).
    private static async Task<(Template template, PhishingPage phishing, EducationalPage edu, Target alvo)>
        SemearRecursosAsync(AppDbContext context, Guid tenantId)
    {
        var template = new Template { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Isca", Assunto = "A", RemetenteNome = "R", RemetenteEmail = "r@t.com", CorpoHtml = "x" };
        var phishing = new PhishingPage { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Pagina", HtmlCaptura = "x" };
        var edu = new EducationalPage { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Edu", HtmlEducacional = "x" };
        var alvo = new Target { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Alvo", Email = "a@t.com", Departamento = "TI" };
        context.Templates.Add(template);
        context.PhishingPages.Add(phishing);
        context.EducationalPages.Add(edu);
        context.Targets.Add(alvo);
        await context.SaveChangesAsync();
        return (template, phishing, edu, alvo);
    }

    // BINDING REAL DA API: exercita a desserialização (System.Text.Json, Web defaults) que
    // o [FromBody] usa. Se a chave camelCase 'dataFim' do front não casasse com a
    // propriedade PascalCase 'DataFim', o prazo chegaria null no controller — exatamente o
    // sintoma "salva Sem prazo". Este teste fecha a lacuna que os testes que montam o DTO
    // direto em C# não cobrem.
    [Fact]
    public void CampaignInputDto_DesserializaDataFim_DeJsonCamelCase()
    {
        const string json = @"{
            ""nomeCampanha"": ""C"",
            ""dataInicio"": ""2030-01-15T12:00:00.000Z"",
            ""dataFim"": ""2030-01-20T18:30:00.000Z"",
            ""emailTemplateId"": ""11111111-1111-1111-1111-111111111111"",
            ""landingPageId"": ""22222222-2222-2222-2222-222222222222"",
            ""educationalPageId"": ""33333333-3333-3333-3333-333333333333"",
            ""targetIds"": []
        }";

        var opts = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        var dto = System.Text.Json.JsonSerializer.Deserialize<CampaignInputDto>(json, opts);

        Assert.NotNull(dto);
        Assert.True(dto!.DataFim.HasValue);
        Assert.Equal(new DateTime(2030, 1, 20, 18, 30, 0, DateTimeKind.Utc), dto.DataFim!.Value.ToUniversalTime());
    }

    // POST com DataFim → persiste no banco E reaparece na leitura por id (mesma data).
    [Fact]
    public async Task PostCampaign_ComDataFim_PersisteEReapareceNoGet()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenantId = Guid.NewGuid();
        tenantProvider.TenantIdAtivo = tenantId;
        context.Tenants.Add(new Tenant { Id = tenantId, NomeEmpresa = "E", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze });
        await context.SaveChangesAsync();
        var (template, phishing, edu, alvo) = await SemearRecursosAsync(context, tenantId);

        var controller = new CampaignsController(context, tenantProvider);
        var dataFim = new DateTime(2030, 9, 10, 22, 0, 0, DateTimeKind.Utc);
        var input = new CampaignInputDto
        {
            NomeCampanha = "Com prazo",
            DataInicio = new DateTime(2030, 9, 1, 12, 0, 0, DateTimeKind.Utc),
            DataFim = dataFim,
            EmailTemplateId = template.Id,
            LandingPageId = phishing.Id,
            EducationalPageId = edu.Id,
            TargetIds = new List<Guid> { alvo.Id }
        };

        // Act
        var post = await controller.PostCampaign(input);

        // Assert: persistiu no banco...
        Assert.IsType<CreatedAtActionResult>(post);
        var salva = await context.Campaigns.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(dataFim, salva.DataFim);

        // ...e reaparece na leitura por id com a MESMA data (não vira Sem prazo).
        var get = await controller.GetCampaign(salva.Id);
        var ok = Assert.IsType<OkObjectResult>(get.Result);
        var prop = ok.Value!.GetType().GetProperty("dataFim");
        Assert.NotNull(prop);
        Assert.Equal(dataFim, (DateTime?)prop!.GetValue(ok.Value));
    }

    // PUT definindo um prazo numa campanha que estava SEM prazo → o novo DataFim persiste.
    [Fact]
    public async Task PutCampaign_DefinindoDataFim_PersisteONovoPrazo()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenantId = Guid.NewGuid();
        tenantProvider.TenantIdAtivo = tenantId;
        context.Tenants.Add(new Tenant { Id = tenantId, NomeEmpresa = "E", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze });
        await context.SaveChangesAsync();
        var (template, phishing, edu, alvo) = await SemearRecursosAsync(context, tenantId);

        var campanha = new Campaign
        {
            Id = Guid.NewGuid(), TenantId = tenantId, NomeCampanha = "Sem prazo ainda",
            Status = CampaignStatus.Rascunho, DataInicio = DateTime.UtcNow.AddHours(1), DataFim = null,
            EmailTemplateId = template.Id, LandingPageId = phishing.Id, EducationalPageId = edu.Id,
            CriadoEm = DateTime.UtcNow, Targets = new List<Target> { alvo }
        };
        context.Campaigns.Add(campanha);
        await context.SaveChangesAsync();

        var controller = new CampaignsController(context, tenantProvider);
        var novoPrazo = new DateTime(2031, 2, 2, 8, 30, 0, DateTimeKind.Utc);
        var input = new CampaignInputDto
        {
            NomeCampanha = "Sem prazo ainda",
            DataInicio = campanha.DataInicio,
            DataFim = novoPrazo,
            EmailTemplateId = template.Id,
            LandingPageId = phishing.Id,
            EducationalPageId = edu.Id,
            TargetIds = new List<Guid> { alvo.Id }
        };

        // Act
        var put = await controller.PutCampaign(campanha.Id, input);

        // Assert
        Assert.IsType<NoContentResult>(put);
        var atualizada = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == campanha.Id);
        Assert.Equal(novoPrazo, atualizada.DataFim);
    }

    [Fact]
    public async Task Ativar_ComDataInicioNoFuturo_TransicionaParaAgendadaSemDisparar()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var campanha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(2));

        var controller = new CampaignsController(context, tenantProvider);

        // Act
        var resultado = await controller.AtivarCampanha(campanha.Id);

        // Assert: status vira "Agendada" (sem disparo na thread HTTP).
        Assert.IsType<OkObjectResult>(resultado);

        var persistida = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == campanha.Id);
        Assert.Equal(CampaignStatus.Agendada, persistida.Status);
    }

    [Fact]
    public async Task Ativar_ComDataInicioNoPassado_FazClaimProcessandoSemDispararNaThreadHttp()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var campanha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));

        var controller = new CampaignsController(context, tenantProvider);

        // Act
        var resultado = await controller.AtivarCampanha(campanha.Id);

        // Assert: o endpoint NÃO dispara e-mails (retorna 202 Accepted) e faz o CLAIM,
        // deixando a campanha em "Processando" para o worker enviar de forma assíncrona.
        Assert.IsType<AcceptedResult>(resultado);

        var persistida = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == campanha.Id);
        Assert.Equal(CampaignStatus.Processando, persistida.Status);
    }

    [Fact]
    public async Task Ativar_CampanhaJaAtivada_RetornaBadRequest()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var campanha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(2));
        campanha.Status = CampaignStatus.Agendada; // já agendada
        await context.SaveChangesAsync();

        var controller = new CampaignsController(context, tenantProvider);

        // Act
        var resultado = await controller.AtivarCampanha(campanha.Id);

        // Assert: só Rascunho pode ser ativada.
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Worker_ProcessaApenasAgendadasNoHorario_IgnorandoRascunho()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // Rascunho no passado — deve ser IGNORADA pelo worker.
        var rascunho = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-10));
        // Agendada no passado — DEVE ser disparada.
        var agendadaPassado = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-10));
        agendadaPassado.Status = CampaignStatus.Agendada;
        // Agendada no futuro — ainda NÃO no horário.
        var agendadaFuturo = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(3));
        agendadaFuturo.Status = CampaignStatus.Agendada;
        await context.SaveChangesAsync();

        var dispatch = new FakeDispatchService();
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), NullLogger<CampaignSchedulerWorker>.Instance);

        // Act
        await worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None);

        // Assert: apenas a "Agendada no passado" foi disparada.
        Assert.Equal(1, dispatch.Chamadas);

        var rascunhoDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == rascunho.Id);
        var agendadaPassadoDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == agendadaPassado.Id);
        var agendadaFuturoDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == agendadaFuturo.Id);

        Assert.Equal(CampaignStatus.Rascunho, rascunhoDb.Status);       // intocada
        Assert.Equal(CampaignStatus.EmAndamento, agendadaPassadoDb.Status); // disparada
        Assert.Equal(CampaignStatus.Agendada, agendadaFuturoDb.Status);  // ainda aguardando
    }

    [Fact]
    public async Task Worker_RetomaCampanhaEmProcessando_AposReinicioDoContainer()
    {
        // Arrange: simula um restart no meio de um lote — a campanha ficou "Processando"
        // (claim já feito) e o envio não terminou. O worker deve REPROCESSÁ-LA.
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // DataInicio no FUTURO de propósito: prova que "Processando" é elegível pelo
        // próprio status (retomada), independentemente do horário de início.
        var processando = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(5));
        processando.Status = CampaignStatus.Processando;
        await context.SaveChangesAsync();

        var dispatch = new FakeDispatchService();
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), NullLogger<CampaignSchedulerWorker>.Instance);

        // Act
        await worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None);

        // Assert: a campanha "Processando" foi retomada e concluída.
        Assert.Equal(1, dispatch.Chamadas);
        var db = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == processando.Id);
        Assert.Equal(CampaignStatus.EmAndamento, db.Status);
    }

    [Fact]
    public async Task Worker_FinalizaEmAndamentoComDataFimVencida_MantendoAsDemais()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // Em Andamento com DataFim VENCIDA → deve ser Finalizada.
        var vencida = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(-3));
        vencida.Status = CampaignStatus.EmAndamento;
        vencida.DataFim = DateTime.UtcNow.AddMinutes(-1);
        // Em Andamento com DataFim no FUTURO → permanece coletando.
        var coletando = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(-3));
        coletando.Status = CampaignStatus.EmAndamento;
        coletando.DataFim = DateTime.UtcNow.AddHours(2);
        // Em Andamento SEM DataFim (null) → permanece ativa indefinidamente.
        var semPrazo = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(-3));
        semPrazo.Status = CampaignStatus.EmAndamento;
        semPrazo.DataFim = null;
        await context.SaveChangesAsync();

        var dispatch = new FakeDispatchService();
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), NullLogger<CampaignSchedulerWorker>.Instance);

        // Act
        await worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None);

        // Assert: nenhuma dessas está "Agendada", então nada é disparado.
        Assert.Equal(0, dispatch.Chamadas);

        var vencidaDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == vencida.Id);
        var coletandoDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == coletando.Id);
        var semPrazoDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == semPrazo.Id);

        Assert.Equal(CampaignStatus.Finalizada, vencidaDb.Status);     // encerrada
        Assert.Equal(CampaignStatus.EmAndamento, coletandoDb.Status);  // ainda coletando
        Assert.Equal(CampaignStatus.EmAndamento, semPrazoDb.Status);   // sem prazo, segue ativa
    }

    // ------------------------------------------------------------------------------------
    // TIMEZONE: a data de agendamento recebida é normalizada para UTC na fronteira da API.
    // Sem isto, um DateTime 'Unspecified' (ex.: chamada via Swagger sem 'Z') seria rejeitado
    // pelo Npgsql ao gravar em 'timestamp with time zone', e/ou desalinharia a comparação do
    // worker (que usa DateTime.UtcNow). O provedor InMemory preserva o Kind, então o teste
    // enxerga a normalização diretamente.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task PostCampaign_NormalizaDatasParaUtc_MesmoRecebendoSemFuso()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenantId = Guid.NewGuid();
        tenantProvider.TenantIdAtivo = tenantId;

        var tenant = new Tenant { Id = tenantId, NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        var template = new Template { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Isca", Assunto = "A", RemetenteNome = "R", RemetenteEmail = "r@t.com", CorpoHtml = "hbomax-redefinicao-senha" };
        var phishing = new PhishingPage { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Pagina", HtmlCaptura = "x" };
        var edu = new EducationalPage { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Edu", HtmlEducacional = "x" };
        var alvo = new Target { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Alvo", Email = "a@t.com", Departamento = "TI" };
        context.Tenants.Add(tenant);
        context.Templates.Add(template);
        context.PhishingPages.Add(phishing);
        context.EducationalPages.Add(edu);
        context.Targets.Add(alvo);
        await context.SaveChangesAsync();

        var controller = new CampaignsController(context, tenantProvider);

        // Datas SEM fuso (Kind=Unspecified) — o cenário problemático.
        var input = new CampaignInputDto
        {
            NomeCampanha = "Nova",
            DataInicio = new DateTime(2030, 1, 15, 12, 0, 0, DateTimeKind.Unspecified),
            DataFim = new DateTime(2030, 1, 16, 12, 0, 0, DateTimeKind.Unspecified),
            EmailTemplateId = template.Id,
            LandingPageId = phishing.Id,
            EducationalPageId = edu.Id,
            TargetIds = new List<Guid> { alvo.Id }
        };

        // Act
        var resultado = await controller.PostCampaign(input);

        // Assert: persistida como UTC (Kind e valor preservado — Unspecified assume UTC).
        Assert.IsType<CreatedAtActionResult>(resultado);
        var persistida = await context.Campaigns.IgnoreQueryFilters().FirstAsync();
        Assert.Equal(DateTimeKind.Utc, persistida.DataInicio.Kind);
        Assert.Equal(new DateTime(2030, 1, 15, 12, 0, 0, DateTimeKind.Utc), persistida.DataInicio);
        Assert.Equal(DateTimeKind.Utc, persistida.DataFim!.Value.Kind);
    }

    // ------------------------------------------------------------------------------------
    // LOG ROBUSTO: quando o disparo falha, o worker CLASSIFICA a causa provável no log para
    // isolar, sem ler stack trace, se o problema foi autenticação SMTP vs. configuração/fila.
    // A campanha reivindicada (Processando) NÃO regride e é reprocessada no próximo ciclo.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task Worker_FalhaAutenticacaoSmtp_ClassificaNoLog_ECampanhaFicaProcessando()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var agendada = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));
        agendada.Status = CampaignStatus.Agendada;
        await context.SaveChangesAsync();

        var logger = new CapturingLogger<CampaignSchedulerWorker>();
        var dispatch = new ThrowingDispatchService(new MailKit.Security.AuthenticationException("credenciais inválidas"));
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), logger);

        // Act
        await worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None);

        // Assert: claim feito (Agendada→Processando), disparo falhou → permanece Processando.
        var db = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == agendada.Id);
        Assert.Equal(CampaignStatus.Processando, db.Status);

        // O log isola a categoria: AUTENTICAÇÃO SMTP.
        Assert.Contains(logger.Mensagens, m => m.Contains("AUTENTICAÇÃO SMTP"));
    }

    [Fact]
    public async Task Worker_FalhaDeConfiguracaoOuFila_ClassificaComoConfiguracao()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var agendada = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));
        agendada.Status = CampaignStatus.Agendada;
        await context.SaveChangesAsync();

        var logger = new CapturingLogger<CampaignSchedulerWorker>();
        // InvalidOperationException é o que o CampaignDispatchService lança quando o SMTP não
        // está configurado para o tenant, ou template/alvos estão ausentes.
        var dispatch = new ThrowingDispatchService(new InvalidOperationException("Configuração SMTP não encontrada para o tenant da campanha."));
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), logger);

        // Act
        await worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None);

        // Assert
        Assert.Contains(logger.Mensagens, m => m.Contains("CONFIGURAÇÃO/FILA"));
    }

    // ------------------------------------------------------------------------------------
    // LISTAGEM: a coluna "Encerramento da Coleta" ficava vazia porque a projeção do GET
    // (lista) não incluía DataFim — só o GET por id incluía. A listagem DEVE trazer o campo
    // mesmo em Rascunho (a data já existe; só passa a valer quando a campanha é ativada).
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task GetCampaigns_IncluiDataFim_MesmoEmRascunho()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenantId = Guid.NewGuid();
        tenantProvider.TenantIdAtivo = tenantId;

        var template = new Template { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Isca", Assunto = "A", RemetenteNome = "R", RemetenteEmail = "r@t.com", CorpoHtml = "x" };
        var phishing = new PhishingPage { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Pagina", HtmlCaptura = "x" };
        var edu = new EducationalPage { Id = Guid.NewGuid(), TenantId = tenantId, Nome = "Edu", HtmlEducacional = "x" };
        var dataFim = new DateTime(2030, 5, 20, 10, 0, 0, DateTimeKind.Utc);
        var campanha = new Campaign
        {
            Id = Guid.NewGuid(), TenantId = tenantId, NomeCampanha = "Rascunho com prazo",
            Status = CampaignStatus.Rascunho, DataInicio = DateTime.UtcNow.AddHours(1), DataFim = dataFim,
            EmailTemplateId = template.Id, LandingPageId = phishing.Id, EducationalPageId = edu.Id,
            CriadoEm = DateTime.UtcNow, Template = template, PhishingPage = phishing, EducationalPage = edu
        };
        context.Templates.Add(template);
        context.PhishingPages.Add(phishing);
        context.EducationalPages.Add(edu);
        context.Campaigns.Add(campanha);
        await context.SaveChangesAsync();

        var controller = new CampaignsController(context, tenantProvider);

        // Act
        var resultado = await controller.GetCampaigns();

        // Assert: o item projetado expõe 'dataFim' com o valor persistido (não some em Rascunho).
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var lista = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        var item = lista.Cast<object>().Single();
        var prop = item.GetType().GetProperty("dataFim");
        Assert.NotNull(prop);
        Assert.Equal(dataFim, (DateTime?)prop!.GetValue(item));
    }

    [Fact]
    public void ClassificarFalhaDisparo_MapeiaCadaCategoria()
    {
        // Contrato do classificador de falhas (usado no log robusto do worker).
        Assert.Contains("AUTENTICAÇÃO SMTP",
            CampaignSchedulerWorker.ClassificarFalhaDisparo(new MailKit.Security.AuthenticationException()));
        Assert.Contains("CONEXÃO",
            CampaignSchedulerWorker.ClassificarFalhaDisparo(new System.Net.Sockets.SocketException()));
        Assert.Contains("CONFIGURAÇÃO/FILA",
            CampaignSchedulerWorker.ClassificarFalhaDisparo(new InvalidOperationException()));
        Assert.Contains("NÃO CLASSIFICADA",
            CampaignSchedulerWorker.ClassificarFalhaDisparo(new Exception()));
    }
}
