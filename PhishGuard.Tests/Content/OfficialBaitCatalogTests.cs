using System.Linq;
using PhishGuard.Backend.Content;
using Xunit;

namespace PhishGuard.Tests.Content;

// Regressão da amarração da isca de e-mail da Amazon: o disparo persiste apenas o
// IDENTIFICADOR ('amazon-notificacao-seguranca') e o OfficialBaitCatalog precisa
// resolvê-lo para o HTML embutido em Resources/OfficialBaits/*.html. Se o recurso
// não estiver embutido/mapeado, ResolveHtml cai no fallback e o e-mail sai com o
// texto literal do id — foi exatamente o sintoma reportado.
public class OfficialBaitCatalogTests
{
    private const string ChaveSeguranca = "amazon-notificacao-seguranca";

    [Fact]
    public void ResolveHtml_ComChaveDaIscaDeSeguranca_RetornaHtmlEmbutido()
    {
        var html = OfficialBaitCatalog.ResolveHtml(ChaveSeguranca);

        // Não pode devolver o próprio id (fallback) — tem que ser o HTML real.
        Assert.NotEqual(ChaveSeguranca, html);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Alerta de segurança", html);
        Assert.Contains("{{LINK_PHISHING}}", html);
    }

    [Fact]
    public void IsKnownId_ReconheceIscaDeSeguranca()
    {
        Assert.True(OfficialBaitCatalog.IsKnownId(ChaveSeguranca));
        Assert.Contains(ChaveSeguranca, OfficialBaitCatalog.Ids);
    }

    // Regressão do sintoma reportado: o e-mail "Microsft 365" saía com o texto literal
    // 'microcorp-expiracao-senha' porque o recurso embutido não existia. Agora deve
    // resolver para o HTML real (isca de expiração de senha corporativa).
    [Fact]
    public void ResolveHtml_ComChaveMicrosft365_RetornaHtmlEmbutido()
    {
        const string chave = "microcorp-expiracao-senha";
        var html = OfficialBaitCatalog.ResolveHtml(chave);

        Assert.NotEqual(chave, html);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Microsft 365", html);
        Assert.Contains("{{LINK_PHISHING}}", html);
        Assert.True(OfficialBaitCatalog.IsKnownId(chave));
    }

    // Regressão da nova isca "Mercado Liv" (alerta de novo acesso). O disparo persiste
    // apenas o id 'mercadoliv-novo-acesso'; o catálogo precisa resolvê-lo para o HTML
    // embutido — que referencia a logo inline via CID (cid:logo-mercadoliv) e a data
    // dinâmica {{DATA_ACESSO}}.
    [Fact]
    public void ResolveHtml_ComChaveMercadoLiv_RetornaHtmlEmbutido()
    {
        const string chave = "mercadoliv-novo-acesso";
        var html = OfficialBaitCatalog.ResolveHtml(chave);

        Assert.NotEqual(chave, html);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Mercado Liv", html);
        Assert.Contains("cid:logo-mercadoliv", html);
        Assert.Contains("{{LINK_PHISHING}}", html);
        Assert.Contains("{{DATA_ACESSO}}", html);
        Assert.True(OfficialBaitCatalog.IsKnownId(chave));
    }

    [Fact]
    public void ResolveHtml_ComValorDesconhecido_MantemFallback()
    {
        const string legado = "<html>registro legado com HTML bruto</html>";
        Assert.Equal(legado, OfficialBaitCatalog.ResolveHtml(legado));
    }
}
