using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.TestDoubles;

// Base compartilhada pelos testes de campanha (CampaignsControllerTests + o novo
// CampaignSchedulerWorkerTests). Concentra os test doubles e os helpers de semeadura
// num só lugar — extração do Passo 7, eliminando a duplicação que existia quando os
// testes de worker moravam dentro de CampaignsControllerTests.
public abstract class CampaignTestBase
{
    // Provider de tenant mutável: permite "virar" o tenant ativo entre operações.
    protected sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantIdAtivo { get; set; }
        public Guid GetTenantId() => TenantIdAtivo;
        public Guid GetCurrentTenantId() => TenantIdAtivo;
    }

    // Substitui o disparo real (SMTP): registra a chamada e aplica a mesma transição
    // de status que o serviço real (→ Em Andamento).
    protected sealed class FakeDispatchService : ICampaignDispatchService
    {
        public int Chamadas { get; private set; }
        public Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            Chamadas++;
            campaign.Status = CampaignStatus.EmAndamento;
            return Task.CompletedTask;
        }
    }

    // Dispatcher que SEMPRE falha com uma exceção dada — exercita a classificação de
    // falha (log robusto) e a propagação de cancelamento do worker sem SMTP real.
    protected sealed class ThrowingDispatchService : ICampaignDispatchService
    {
        private readonly Exception _erro;
        public ThrowingDispatchService(Exception erro) => _erro = erro;
        public Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default) => throw _erro;
    }

    // Falha o disparo de UMA campanha específica e conclui as demais — prova o
    // isolamento entre campanhas (a falha de uma não interrompe o lote).
    protected sealed class SelectiveDispatchService : ICampaignDispatchService
    {
        private readonly Guid _idQueFalha;
        private readonly Exception _erro;
        public List<Guid> Disparadas { get; } = new();
        public SelectiveDispatchService(Guid idQueFalha, Exception erro)
        {
            _idQueFalha = idQueFalha;
            _erro = erro;
        }
        public Task DispatchAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            if (campaign.Id == _idQueFalha) throw _erro;
            Disparadas.Add(campaign.Id);
            campaign.Status = CampaignStatus.EmAndamento;
            return Task.CompletedTask;
        }
    }

    // Logger que captura as mensagens FORMATADAS, para asserir a categoria de falha logada.
    protected sealed class CapturingLogger<T> : ILogger<T>
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
    protected sealed class SingleScopeFactory : IServiceScopeFactory, IServiceScope, IServiceProvider
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

    protected static (AppDbContext context, FakeTenantProvider tenantProvider) CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantProvider = new FakeTenantProvider();
        return (new AppDbContext(options, tenantProvider), tenantProvider);
    }

    // Cria uma campanha em Rascunho (com 1 alvo e 1 template) no tenant informado.
    protected static async Task<Campaign> SemearCampanhaRascunhoAsync(
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
    protected static async Task<(Template template, PhishingPage phishing, EducationalPage edu, Target alvo)>
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
}
