using System;
using PhishGuard.Backend.Services;
using Xunit;

namespace PhishGuard.Tests.Services;

// Valida a política anti-XSS: remove executáveis (script/handlers/URIs javascript),
// mantém a estrutura visual/forms das landings e preserva placeholders de disparo.
public class HtmlSanitizationServiceTests
{
    private readonly IHtmlSanitizationService _sanitizer = new HtmlSanitizationService();

    [Fact]
    public void Remove_TagScript()
    {
        var limpo = _sanitizer.Sanitize("<div>ok</div><script>alert(document.cookie)</script>");
        Assert.DoesNotContain("<script", limpo, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ok", limpo);
    }

    [Fact]
    public void Remove_HandlerInline_OnError()
    {
        var limpo = _sanitizer.Sanitize("<img src=\"x\" onerror=\"alert(1)\" />");
        Assert.DoesNotContain("onerror", limpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Remove_UriJavascript()
    {
        var limpo = _sanitizer.Sanitize("<a href=\"javascript:alert(1)\">x</a>");
        Assert.DoesNotContain("javascript:", limpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Preserva_Formularios_E_Estilo()
    {
        var html = "<style>.x{color:red}</style>" +
                   "<form action=\"/x\" method=\"post\"><input type=\"password\" name=\"senha\" /></form>";
        var limpo = _sanitizer.Sanitize(html);
        Assert.Contains("<form", limpo, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<input", limpo, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<style", limpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Preserva_ClassesDosEstilosSemRestaurarHandlersExecutaveis()
    {
        var html = "<style>.mc-card{background:#fff;padding:36px}</style>"
            + "<div class=\"mc-card\" onclick=\"alert(1)\">Cartão</div>";
        var clean = _sanitizer.Sanitize(html);

        Assert.Contains("class=\"mc-card\"", clean);
        Assert.Contains(".mc-card", clean);
        Assert.DoesNotContain("onclick", clean, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(clean, _sanitizer.Sanitize(clean));
    }

    [Theory]
    [InlineData("{{LINK}}")]
    [InlineData("{{LINK_PHISHING}}")]
    public void Preserva_Placeholder_De_Disparo_Em_Href(string placeholder)
    {
        var limpo = _sanitizer.Sanitize($"<a href=\"{placeholder}\">Clique</a>");
        Assert.Contains($"href=\"{placeholder}\"", limpo);
    }

    [Theory]
    [InlineData("javascript:alert(1)//{{NOME}}")]
    [InlineData("JaVaScRiPt:alert(1)//{{LINK}}")]
    [InlineData("javascript&#58;alert(1)//{{LINK_PHISHING}}")]
    [InlineData("javascript:alert(1)//{{UNKNOWN}}")]
    [InlineData("data:text/html,<script>alert(1)</script>{{LINK}}")]
    [InlineData("vbscript:msgbox(1)'{{LINK}}")]
    [InlineData("{{UNKNOWN}}")]
    [InlineData("https://example.test/{{LINK}}")]
    [InlineData("{{LINK}}/suffix")]
    public void Placeholder_NaoRestauraUrlExecutavelOuMarcadorParcial(string url)
    {
        var limpo = _sanitizer.Sanitize($"<a href=\"{url}\">Clique</a>"
            + $"<form action=\"{url}\"><input name=\"campo\"></form>");

        Assert.DoesNotContain("href=", limpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("action=", limpo, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Clique", limpo);
        Assert.Contains("<form", limpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Preserva_UrlHttps_SemPlaceholder()
    {
        var limpo = _sanitizer.Sanitize("<a href=\"https://example.test/training\">Clique</a>");
        Assert.Contains("href=\"https://example.test/training\"", limpo);
    }

    [Theory]
    [InlineData("logo-bhomax")]
    [InlineData("logo-mercadoliv")]
    public void Preserva_Cid_SomenteParaLogosEmbutidosHomologados(string token)
    {
        var clean = _sanitizer.Sanitize($"<img src=\"cid:{token}\"><img src=\"cid:unknown\"><img src=\"cid:{token}%00\">");
        Assert.Contains($"src=\"cid:{token}\"", clean);
        Assert.DoesNotContain("cid:unknown", clean);
        Assert.DoesNotContain("%00", clean);
    }

    [Fact]
    public void Preserva_SvgGeometricoDosRecursosOficiaisComAcessibilidade()
    {
        var html = "<button aria-label=\"Exibir senha\"><svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" aria-hidden=\"true\">"
            + "<path d=\"M2 12L12 2L22 12Z\" stroke-linecap=\"round\"></path><circle cx=\"12\" cy=\"12\" r=\"3\"></circle>"
            + "<line x1=\"0\" y1=\"0\" x2=\"24\" y2=\"24\"></line></svg></button>";
        var clean = _sanitizer.Sanitize(html);
        Assert.Contains("<svg", clean);
        Assert.Contains("viewBox=\"0 0 24 24\"", clean);
        Assert.Contains("<path", clean);
        Assert.Contains("<circle", clean);
        Assert.Contains("<line", clean);
        Assert.Contains("aria-label=\"Exibir senha\"", clean);
        Assert.Contains("stroke=\"currentColor\"", clean);
    }

    [Fact]
    public void SvgHostil_NaoPreservaExecucaoLinksAnimacaoOuDocumentosExternos()
    {
        var clean = _sanitizer.Sanitize("<svg onload=\"alert(1)\" href=\"https://evil.test/image\" xlink:href=\"javascript:alert(1)\">"
            + "<script>alert(1)</script><foreignObject><iframe src=\"https://evil.test\"></iframe></foreignObject>"
            + "<use href=\"https://evil.test#id\"></use><animate attributeName=\"href\" values=\"javascript:alert(1)\"></animate>"
            + "<path d=\"M0 0L1 1\" onmouseover=\"alert(1)\" href=\"https://evil.test\" fill=\"url(https://evil.test/paint)\"></path>"
            + "<a href=\"https://evil.test\">Link</a></svg>");
        Assert.Contains("<svg", clean);
        Assert.Contains("<path", clean);
        foreach (var forbidden in new[] { "onload", "onmouseover", "<script", "<foreignObject", "<iframe", "<use", "<animate", "href=", "javascript:", "url(" })
            Assert.DoesNotContain(forbidden, clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NaoAltera_IdDeIscaOficial_SemTags()
    {
        const string id = "amazon-notificacao-seguranca";
        Assert.Equal(id, _sanitizer.Sanitize(id));
    }
}
