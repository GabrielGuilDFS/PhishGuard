using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PhishGuard.Backend.BackgroundServices;
using PhishGuard.Backend.Models;
using PhishGuard.Tests.TestDoubles;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PhishGuard.Tests.BackgroundServices;

// Testes do CampaignSchedulerWorker (orquestração de disparo/finalização em background).
// Movidos de CampaignsControllerTests para a pasta-espelho de BackgroundServices/ (Passo 7),
// reusando os test doubles e helpers de CampaignTestBase. Exercitam o método interno
// ProcessarCampanhasElegiveisAsync (via InternalsVisibleTo) sem SMTP nem timer real.
public class CampaignSchedulerWorkerTests : CampaignTestBase
{
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

    // ------------------------------------------------------------------------------------
    // NOVO (Passo 7) — ISOLAMENTO ENTRE CAMPANHAS: a falha no disparo de UMA campanha não
    // interrompe o processamento das demais nem derruba o ciclo (try/catch por item).
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task Worker_FalhaDeUmaCampanha_NaoInterrompeAsOutras()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var comFalha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));
        comFalha.Status = CampaignStatus.Agendada;
        var saudavel = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));
        saudavel.Status = CampaignStatus.Agendada;
        await context.SaveChangesAsync();

        var logger = new CapturingLogger<CampaignSchedulerWorker>();
        var dispatch = new SelectiveDispatchService(comFalha.Id,
            new InvalidOperationException("Configuração SMTP não encontrada para o tenant da campanha."));
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), logger);

        // Act: NÃO deve lançar — a falha de uma campanha é isolada das demais.
        await worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None);

        // Assert: a saudável foi disparada e concluída; a com falha ficou em Processando.
        Assert.Contains(saudavel.Id, dispatch.Disparadas);
        var saudavelDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == saudavel.Id);
        var comFalhaDb = await context.Campaigns.IgnoreQueryFilters().FirstAsync(c => c.Id == comFalha.Id);
        Assert.Equal(CampaignStatus.EmAndamento, saudavelDb.Status);
        Assert.Equal(CampaignStatus.Processando, comFalhaDb.Status);
        Assert.Contains(logger.Mensagens, m => m.Contains("CONFIGURAÇÃO/FILA"));
    }

    // ------------------------------------------------------------------------------------
    // NOVO (Passo 7) — CANCELAMENTO: o encerramento do host (OperationCanceledException) NÃO
    // é tratado como falha de disparo; PROPAGA para o loop do ExecuteAsync encerrar o worker.
    // ------------------------------------------------------------------------------------
    [Fact]
    public async Task Worker_OperationCanceledException_Propaga()
    {
        // Arrange
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var agendada = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-5));
        agendada.Status = CampaignStatus.Agendada;
        await context.SaveChangesAsync();

        var dispatch = new ThrowingDispatchService(new OperationCanceledException());
        var worker = new CampaignSchedulerWorker(new SingleScopeFactory(context, dispatch), NullLogger<CampaignSchedulerWorker>.Instance);

        // Act + Assert: a exceção de cancelamento propaga (não vira "falha classificada").
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => worker.ProcessarCampanhasElegiveisAsync(CancellationToken.None));
    }
}
