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

    [Fact]
    public void ResolveHtml_ComValorDesconhecido_MantemFallback()
    {
        const string legado = "<html>registro legado com HTML bruto</html>";
        Assert.Equal(legado, OfficialBaitCatalog.ResolveHtml(legado));
    }
}
