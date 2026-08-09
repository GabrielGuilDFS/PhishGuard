using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhishGuard.Backend.Controllers;
using PhishGuard.Backend.DTOs;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;
using PhishGuard.Backend.Services.Delivery;
using PhishGuard.Tests.TestDoubles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PhishGuard.Tests.Controllers;

// Regras de transição de estado das campanhas (Rascunho → Agendada → Processando → Em Andamento)
// pela ótica do CONTROLLER (a API). Os testes do CampaignSchedulerWorker que exercitam essas
// transições no processo de background vivem em BackgroundServices/CampaignSchedulerWorkerTests
// (Passo 7). Os test doubles e helpers de semeadura são compartilhados via CampaignTestBase.
public class CampaignsControllerTests : CampaignTestBase
{
    private sealed class UnreadableSmtpProtector : ISmtpCredentialProtector
    {
        public string Protect(string? plaintext) => plaintext ?? string.Empty;
        public string Unprotect(string? stored) => throw new SmtpOperationalException(
            SmtpOperationalPolicy.CredentialUnreadableCode,
            "Credencial ilegível.");
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
    public async Task Ativar_ComMailtrapSandbox_PermiteApiHttpsQuandoSmtpEstaDesabilitado()
    {
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        var campanha = await SemearCampanhaRascunhoAsync(
            context,
            tenantProvider,
            tenant.Id,
            DateTime.UtcNow.AddMinutes(-5));
        var deliveryConfig = await context.SmtpConfigs.IgnoreQueryFilters()
            .SingleAsync(config => config.TenantId == tenant.Id);
        deliveryConfig.ProviderType = EmailProviderType.ProviderApi;
        deliveryConfig.ApiProvider = ApiProviderName.MailtrapSandbox;
        deliveryConfig.ApiAccountIdentifier = "4475065";
        deliveryConfig.EncryptedApiKey = "token-api-protegido";
        deliveryConfig.SenderEmail = "remetente.simulado@phishguard.test";
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:SmtpTransportEnabled"] = "false",
                ["AppSettings:SmtpTransportDisabledReason"] = "SMTP bloqueado no Render."
            })
            .Build();
        var controller = new CampaignsController(context, tenantProvider, configuration);

        var resultado = await controller.AtivarCampanha(campanha.Id);

        Assert.IsType<AcceptedResult>(resultado);
        var persistida = await context.Campaigns.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == campanha.Id);
        Assert.Equal(CampaignStatus.Processando, persistida.Status);
    }

    [Fact]
    public async Task Ativar_ComSmtp_ContinuaBloqueadoQuandoTransporteEstaDesabilitado()
    {
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        var campanha = await SemearCampanhaRascunhoAsync(
            context,
            tenantProvider,
            tenant.Id,
            DateTime.UtcNow.AddMinutes(-5));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:SmtpTransportEnabled"] = "false"
            })
            .Build();
        var controller = new CampaignsController(context, tenantProvider, configuration);

        var resultado = await controller.AtivarCampanha(campanha.Id);

        Assert.IsType<ConflictObjectResult>(resultado);
        Assert.Equal(CampaignStatus.Rascunho, campanha.Status);
    }

    [Fact]
    public async Task RetryDispatch_ComMailtrapSandbox_PermiteApiHttpsQuandoSmtpEstaDesabilitado()
    {
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        var campanha = await SemearCampanhaRascunhoAsync(
            context,
            tenantProvider,
            tenant.Id,
            DateTime.UtcNow.AddMinutes(-5));
        campanha.Status = CampaignStatus.FalhaNoDisparo;
        var deliveryConfig = await context.SmtpConfigs.IgnoreQueryFilters()
            .SingleAsync(config => config.TenantId == tenant.Id);
        deliveryConfig.ProviderType = EmailProviderType.ProviderApi;
        deliveryConfig.ApiProvider = ApiProviderName.MailtrapSandbox;
        deliveryConfig.ApiAccountIdentifier = "4475065";
        deliveryConfig.EncryptedApiKey = "token-api-protegido";
        deliveryConfig.SenderEmail = "remetente.simulado@phishguard.test";
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:SmtpTransportEnabled"] = "false"
            })
            .Build();
        var controller = new CampaignsController(context, tenantProvider, configuration);

        var resultado = await controller.RetryDispatch(campanha.Id);

        Assert.IsType<AcceptedResult>(resultado);
        Assert.Equal(CampaignStatus.Processando, campanha.Status);
    }

    [Fact]
    public async Task Ativar_SemSmtp_RetornaConflictEMantemRascunho()
    {
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        var campanha = await SemearCampanhaRascunhoAsync(context, tenantProvider, tenant.Id, DateTime.UtcNow.AddMinutes(-1));
        var smtp = await context.SmtpConfigs.IgnoreQueryFilters().SingleAsync(s => s.TenantId == tenant.Id);
        context.SmtpConfigs.Remove(smtp);
        await context.SaveChangesAsync();

        var controller = new CampaignsController(context, tenantProvider);
        var resultado = await controller.AtivarCampanha(campanha.Id);

        Assert.IsType<ConflictObjectResult>(resultado);
        Assert.Equal(CampaignStatus.Rascunho, campanha.Status);
    }

    [Fact]
    public async Task Ativar_ComCredencialSmtpIlegivel_RetornaConflictEMantemRascunho()
    {
        var (context, tenantProvider) = CriarContexto();
        var tenant = new Tenant { Id = Guid.NewGuid(), NomeEmpresa = "Empresa", Cnpj = "11111111000191", Ativo = true, CriadoEm = DateTime.UtcNow, Plano = PlanoTenant.Bronze };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        var campanha = await SemearCampanhaRascunhoAsync(
            context,
            tenantProvider,
            tenant.Id,
            DateTime.UtcNow.AddMinutes(-1));

        var controller = new CampaignsController(
            context,
            tenantProvider,
            smtpCredentialProtector: new UnreadableSmtpProtector());
        var resultado = await controller.AtivarCampanha(campanha.Id);

        Assert.IsType<ConflictObjectResult>(resultado);
        Assert.Equal(CampaignStatus.Rascunho, campanha.Status);
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
}
