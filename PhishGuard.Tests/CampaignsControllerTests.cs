using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PhishGuard.Backend.BackgroundServices;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PhishGuard.Tests;

// Regras de transição de estado das campanhas (Rascunho → Agendada → Em Andamento):
//  - Ativar com DataInicio no FUTURO  → Agendada (sem disparo imediato).
//  - Ativar com DataInicio no PASSADO → Em Andamento (disparo imediato).
//  - O worker de agendamento processa APENAS "Agendada" já no horário; ignora Rascunho.
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

    [Fact]
    public async Task Ativar_ComDataInicioNoFuturo_TransicionaParaAgendadaSemDisparar()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var campanha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddHours(2));

        var dispatch = new FakeDispatchService();
        var controller = new CampaignsController(context, tenantProvider, dispatch);

        // Act
        var resultado = await controller.AtivarCampanha(campanha.Id);

        // Assert: status vira "Agendada" e NENHUM disparo ocorre.
        Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(0, dispatch.Chamadas);

        var persistida = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == campanha.Id);
        Assert.Equal(CampaignStatus.Agendada, persistida.Status);
    }

    [Fact]
    public async Task Ativar_ComDataInicioNoPassado_TransicionaParaEmAndamentoComDisparoImediato()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var campanha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));

        var dispatch = new FakeDispatchService();
        var controller = new CampaignsController(context, tenantProvider, dispatch);

        // Act
        var resultado = await controller.AtivarCampanha(campanha.Id);

        // Assert: disparo imediato e status "Em Andamento".
        Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(1, dispatch.Chamadas);
        Assert.Equal(CampaignStatus.EmAndamento, campanha.Status);
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

        var dispatch = new FakeDispatchService();
        var controller = new CampaignsController(context, tenantProvider, dispatch);

        // Act
        var resultado = await controller.AtivarCampanha(campanha.Id);

        // Assert: só Rascunho pode ser ativada.
        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(0, dispatch.Chamadas);
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
}
