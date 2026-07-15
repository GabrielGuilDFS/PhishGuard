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
    public void Preserva_Placeholder_De_Disparo_Em_Href()
    {
        var limpo = _sanitizer.Sanitize("<a href=\"{{LINK}}\">Clique</a>");
        Assert.Contains("{{LINK}}", limpo);
    }

    [Fact]
    public void NaoAltera_IdDeIscaOficial_SemTags()
    {
        const string id = "amazon-notificacao-seguranca";
        Assert.Equal(id, _sanitizer.Sanitize(id));
    }
}
