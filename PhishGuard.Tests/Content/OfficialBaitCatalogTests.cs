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
        Assert.Contains("Microsoft", html);
        Assert.DoesNotContain("Microsft 365", html);
        Assert.Contains("{{LINK_PHISHING}}", html);
        Assert.True(OfficialBaitCatalog.IsKnownId(chave));
    }

    // O ID original e o usado após 460dc77 precisam resolver o mesmo recurso:
    // campanhas existentes não podem passar a enviar o identificador como texto.
    [Theory]
    [InlineData("mercado-liv-novo-acesso")]
    [InlineData("mercadoliv-novo-acesso")]
    public void ResolveHtml_ComChaveMercadoLiv_RetornaHtmlEmbutido(string chave)
    {
        var html = OfficialBaitCatalog.ResolveHtml(chave);

        Assert.NotEqual(chave, html);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Mercado Livre", html);
        Assert.Contains("cid:logo-mercadoliv", html);
        Assert.Equal(OfficialBaitCatalog.ResolveHtml("mercado-liv-novo-acesso"), html);
        // Variáveis dinâmicas resolvidas no disparo (nome, link e data no fuso BRT).
        Assert.Contains("{{NOME}}", html);
        Assert.Contains("{{LINK_PHISHING}}", html);
        Assert.Contains("{{DATA_ACESSO}}", html);
        Assert.True(OfficialBaitCatalog.IsKnownId(chave));
        Assert.Contains(chave, OfficialBaitCatalog.Ids);
    }

    [Fact]
    public void ResolveHtml_ComValorDesconhecido_MantemFallback()
    {
        const string legado = "<html>registro legado com HTML bruto</html>";
        Assert.Equal(legado, OfficialBaitCatalog.ResolveHtml(legado));
    }
}
