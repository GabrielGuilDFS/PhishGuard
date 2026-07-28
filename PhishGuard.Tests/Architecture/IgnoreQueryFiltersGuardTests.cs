using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace PhishGuard.Tests.Architecture;

// GUARDRAIL ARQUITETURAL (Passo 1) — a governança que uma consulta ao banco NÃO consegue
// expressar: `IgnoreQueryFilters()` fura o Global Query Filter de tenant, então só pode
// aparecer em rotas de SISTEMA / anônimas / pré-tenant. Este teste varre o código-fonte
// do backend e falha se a chamada surgir em qualquer arquivo fora da allowlist — em
// especial nos controllers de ADMINISTRAÇÃO (escopados ao tenant), onde ela vazaria dados.
//
// O caminho do fonte é ancorado por [CallerFilePath] (resolvido em COMPILAÇÃO), que aponta
// para este .cs no checkout — o mesmo lugar onde o teste roda (local e CI).
public class IgnoreQueryFiltersGuardTests
{
    // Arquivos AUTORIZADOS a usar IgnoreQueryFilters, cada um com motivo legítimo:
    //  - CampaignSchedulerWorker / CampaignDispatchService: processos de sistema (sem HttpContext).
    //  - TrackingController: endpoints [AllowAnonymous] (o alvo não tem sessão/tenant).
    //  - Login/Register: fluxo PRÉ-tenant (o JWT/tenant ainda não existe).
    //  - PhishingPages: o GET da landing é [AllowAnonymous] (o alvo acessa sem login).
    private static readonly HashSet<string> Allowlist = new(StringComparer.OrdinalIgnoreCase)
    {
        "BackgroundServices/CampaignSchedulerWorker.cs",
        "Services/CampaignDispatchService.cs",
        "Controllers/TrackingController.cs",
        "Controllers/LoginController.cs",
        "Controllers/RegisterController.cs",
        "Controllers/PhishingPagesController.cs",
    };

    // Controllers de administração (escopados ao tenant) que JAMAIS podem furar o filtro.
    private static readonly string[] AdminControllers =
    {
        "Controllers/CampaignsController.cs",
        "Controllers/TargetsController.cs",
        "Controllers/TemplatesController.cs",
        "Controllers/EducationalPagesController.cs",
        "Controllers/SmtpConfigController.cs",
        "Controllers/DashboardController.cs",
        "Controllers/TenantController.cs",
    };

    private static string BackendDir([CallerFilePath] string thisFile = "")
    {
        // thisFile = <repo>/PhishGuard.Tests/Architecture/IgnoreQueryFiltersGuardTests.cs
        var repoRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", ".."));
        return Path.Combine(repoRoot, "PhishGuard.Backend");
    }

    private static bool UsaIgnoreQueryFilters(string arquivo)
        => File.ReadAllText(arquivo).Contains(".IgnoreQueryFilters(", StringComparison.Ordinal);

    private static string RelativoUnix(string backendDir, string arquivo)
        => Path.GetRelativePath(backendDir, arquivo).Replace('\\', '/');

    // 1) TODO uso de IgnoreQueryFilters no backend está dentro da allowlist. Se um arquivo
    //    novo (ex.: um controller de admin) passar a usar a chamada, este teste quebra.
    [Fact]
    public void IgnoreQueryFilters_SoAparecerNosArquivosAutorizados()
    {
        var backendDir = BackendDir();
        Assert.True(Directory.Exists(backendDir), $"Pasta do backend não encontrada: {backendDir}");

        var infratores = Directory
            .EnumerateFiles(backendDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !RelativoUnix(backendDir, f).StartsWith("Migrations/", StringComparison.OrdinalIgnoreCase))
            .Where(UsaIgnoreQueryFilters)
            .Select(f => RelativoUnix(backendDir, f))
            .Where(rel => !Allowlist.Contains(rel))
            .OrderBy(x => x)
            .ToList();

        Assert.True(
            infratores.Count == 0,
            "IgnoreQueryFilters() encontrado FORA da allowlist (fura o isolamento de tenant): "
                + string.Join(", ", infratores)
                + ". Se o uso for legítimo (rota de sistema/anônima), adicione o arquivo à allowlist com justificativa.");
    }

    // 2) Explícito e legível: cada controller de ADMIN escopado ao tenant está LIMPO.
    [Fact]
    public void ControllersDeAdmin_NuncaUsamIgnoreQueryFilters()
    {
        var backendDir = BackendDir();

        foreach (var rel in AdminControllers)
        {
            var caminho = Path.Combine(backendDir, rel.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(caminho), $"Controller esperado não existe: {rel}");
            Assert.False(
                UsaIgnoreQueryFilters(caminho),
                $"{rel} é um controller de administração (escopado ao tenant) e NÃO pode chamar IgnoreQueryFilters().");
        }
    }

    // 3) A allowlist não pode apodrecer: cada arquivo listado deve existir E ainda conter a
    //    chamada — se um uso legítimo sumir, atualize a lista (evita permitir um caminho morto).
    [Fact]
    public void Allowlist_EstaAtualizada_ArquivosExistemEUsamAChamada()
    {
        var backendDir = BackendDir();

        foreach (var rel in Allowlist)
        {
            var caminho = Path.Combine(backendDir, rel.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(caminho), $"Arquivo da allowlist não existe mais: {rel}");
            Assert.True(
                UsaIgnoreQueryFilters(caminho),
                $"{rel} está na allowlist mas não usa mais IgnoreQueryFilters() — remova-o da lista.");
        }
    }
}
