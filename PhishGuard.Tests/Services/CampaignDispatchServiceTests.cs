using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MimeKit;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Services;

// Cobre o caminho de RESILIÊNCIA do disparo SMTP (retry Polly + registro de "Falha"),
// habilitado pela abstração ISmtpClient/ISmtpClientFactory. Usa o ctor interno do serviço
// para zerar backoff e throttle — os testes rodam em milissegundos, não em segundos.
public class CampaignDispatchServiceTests
{
    private sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
        public Guid GetCurrentTenantId() => Guid.Empty;
    }

    // Protector no-op: devolve a "senha" como está (sem Data Protection real nos testes).
    private sealed class PassthroughProtector : ISmtpCredentialProtector
    {
        public string Protect(string? plaintext) => plaintext ?? string.Empty;
        public string Unprotect(string? stored) => stored ?? string.Empty;
    }

    // Cliente SMTP falso e programável: decide, por endereço de destino, quantas vezes
    // o envio falha antes de suceder (ou se falha sempre). Registra chamadas de Send.
    private sealed class FakeSmtpClient : ISmtpClient
    {
        private readonly Func<string, bool> _deveFalhar;
        public int ConnectCount { get; private set; }
        public int SendCount { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public FakeSmtpClient(Func<string, bool> deveFalhar) => _deveFalhar = deveFalhar;

        public Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken ct)
        {
            ConnectCount++;
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task AuthenticateAsync(string userName, string password, CancellationToken ct)
        {
            IsAuthenticated = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(MimeMessage message, CancellationToken ct)
        {
            SendCount++;
            var destino = message.To.Mailboxes.First().Address;
            if (_deveFalhar(destino))
                throw new InvalidOperationException($"Falha SMTP simulada para {destino}.");
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(bool quit, CancellationToken ct)
        {
            IsConnected = false;
            IsAuthenticated = false;
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }

    private sealed class FakeSmtpClientFactory : ISmtpClientFactory
    {
        private readonly ISmtpClient _client;
        public FakeSmtpClientFactory(ISmtpClient client) => _client = client;
        public ISmtpClient Create() => _client;
    }

    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, new FakeTenantProvider());
    }

    // Monta uma campanha pronta para disparo com N alvos e persiste o SMTP do tenant.
    private static async Task<(AppDbContext ctx, Campaign campaign)> SemearCampanhaAsync(params string[] emailsAlvo)
    {
        var ctx = CriarContexto();
        var tenantId = Guid.NewGuid();

        var template = new Template
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Nome = "Template",
            Assunto = "Assunto",
            RemetenteNome = "Remetente",
            RemetenteEmail = "remetente@empresa.com",
            CorpoHtml = "<p>Olá {{NOME}}, clique em {{LINK}}</p>",
            CriadoEm = DateTime.UtcNow
        };

        var targets = emailsAlvo.Select(e => new Target
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Nome = e.Split('@')[0],
            Email = e
        }).ToList();

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NomeCampanha = "Campanha",
            Status = CampaignStatus.Processando,
            DataInicio = DateTime.UtcNow,
            EmailTemplateId = template.Id,
            Template = template,
            LandingPageId = Guid.NewGuid(),
            EducationalPageId = Guid.NewGuid(),
            Targets = targets,
            CriadoEm = DateTime.UtcNow
        };

        ctx.SmtpConfigs.Add(new SmtpConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Host = "smtp.teste.com",
            Porta = 587,
            Usuario = "user",
            Senha = "senha"
        });
        ctx.Templates.Add(template);
        ctx.Campaigns.Add(campaign);
        await ctx.SaveChangesAsync();

        return (ctx, campaign);
    }

    private static CampaignDispatchService CriarServico(AppDbContext ctx, ISmtpClient client)
    {
        // Ctor interno: backoff zero + throttle zero → sem esperas reais no teste.
        return new CampaignDispatchService(
            ctx,
            NullLogger<CampaignDispatchService>.Instance,
            new PassthroughProtector(),
            new FakeSmtpClientFactory(client),
            backoffProvider: _ => TimeSpan.Zero,
            throttleEntreEnvios: TimeSpan.Zero);
    }

    private static async Task<List<SimulationLog>> LogsAsync(AppDbContext ctx, Guid campaignId, string acao)
        => await ctx.SimulationsLogs.IgnoreQueryFilters()
            .Where(l => l.CampaignId == campaignId && l.Acao == acao)
            .ToListAsync();

    [Fact]
    public async Task Envio_BemSucedido_GravaLogEnvio_E_MoveStatusParaEmAndamento()
    {
        var (ctx, campaign) = await SemearCampanhaAsync("alvo@empresa.com");
        var client = new FakeSmtpClient(_ => false); // nunca falha
        var servico = CriarServico(ctx, client);

        await servico.DispatchAsync(campaign);

        Assert.Equal(1, client.SendCount);
        Assert.Single(await LogsAsync(ctx, campaign.Id, SimulationActions.Envio));
        Assert.Empty(await LogsAsync(ctx, campaign.Id, SimulationActions.Falha));
        Assert.Equal(CampaignStatus.EmAndamento, campaign.Status);
    }

    [Fact]
    public async Task Envio_ComFalhasTransitorias_ReteintaEEntregaSemLogDeFalha()
    {
        var (ctx, campaign) = await SemearCampanhaAsync("alvo@empresa.com");

        // Falha nas 2 primeiras tentativas, sucesso na 3ª (dentro do orçamento de 3 retries).
        var tentativas = 0;
        var client = new FakeSmtpClient(_ => ++tentativas <= 2);
        var servico = CriarServico(ctx, client);

        await servico.DispatchAsync(campaign);

        Assert.Equal(3, client.SendCount); // 2 falhas + 1 sucesso
        Assert.Single(await LogsAsync(ctx, campaign.Id, SimulationActions.Envio));
        Assert.Empty(await LogsAsync(ctx, campaign.Id, SimulationActions.Falha));
    }

    [Fact]
    public async Task Envio_ComFalhaDefinitiva_GravaLogFalha_E_NaoInterrompeOsDemaisAlvos()
    {
        var (ctx, campaign) = await SemearCampanhaAsync("ruim@empresa.com", "bom@empresa.com");

        // O primeiro alvo falha SEMPRE (esgota os retries); o segundo sempre entrega.
        var client = new FakeSmtpClient(destino => destino == "ruim@empresa.com");
        var servico = CriarServico(ctx, client);

        // Não deve lançar: a falha de um alvo não pode derrubar o lote nem o worker.
        await servico.DispatchAsync(campaign);

        var falhas = await LogsAsync(ctx, campaign.Id, SimulationActions.Falha);
        var envios = await LogsAsync(ctx, campaign.Id, SimulationActions.Envio);

        Assert.Single(falhas);
        Assert.Single(envios);
        Assert.Equal(campaign.Targets.First(t => t.Email == "ruim@empresa.com").Id, falhas[0].TargetId);
        Assert.Equal(campaign.Targets.First(t => t.Email == "bom@empresa.com").Id, envios[0].TargetId);
        Assert.Equal(CampaignStatus.EmAndamento, campaign.Status); // lote conclui apesar da falha
    }

    [Fact]
    public async Task Falha_Definitiva_TentaExatamente_QuatroVezes_AntesDeDesistir()
    {
        var (ctx, campaign) = await SemearCampanhaAsync("ruim@empresa.com");
        var client = new FakeSmtpClient(_ => true); // falha sempre
        var servico = CriarServico(ctx, client);

        await servico.DispatchAsync(campaign);

        // 1 tentativa inicial + 3 retries = 4 chamadas de SendAsync antes de registrar Falha.
        Assert.Equal(4, client.SendCount);
        Assert.Single(await LogsAsync(ctx, campaign.Id, SimulationActions.Falha));
    }
}
