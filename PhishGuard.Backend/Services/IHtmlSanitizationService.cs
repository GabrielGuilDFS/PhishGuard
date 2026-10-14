using Ganss.Xss;
using PhishGuard.Backend.Content;
using AngleSharp.Dom;

namespace PhishGuard.Backend.Services
{
    /// <summary>
    /// Sanitiza o HTML bruto de templates de e-mail e landing pages ANTES de persistir,
    /// removendo vetores de XSS (tags &lt;script&gt;, handlers inline on*, URIs javascript:)
    /// e mantendo a estrutura visual segura (layout, estilos e formulários) necessária
    /// para a fidelidade das simulações de phishing.
    /// </summary>
    public interface IHtmlSanitizationService
    {
        string Sanitize(string? html);
    }

    public sealed class HtmlSanitizationService : IHtmlSanitizationService
    {
        private readonly HtmlSanitizer _sanitizer;

        public HtmlSanitizationService()
        {
            _sanitizer = new HtmlSanitizer();

            // Allow-list: além do padrão (que já REMOVE <script> e handlers on*), libera a
            // estrutura visual e de formulário essencial às landings clonadas.
            foreach (var tag in new[]
                     {
                         "style", "form", "input", "button", "label",
                         "select", "option", "textarea", "link", "meta",
                         "svg", "path", "circle", "line"
                     })
            {
                _sanitizer.AllowedTags.Add(tag);
            }

            // Os seletores dos estilos oficiais dependem de class. Preservar esse
            // atributo mantém o layout sem liberar scripts ou handlers de eventos.
            foreach (var attr in new[]
                     {
                         "class", "action", "method", "type", "name", "placeholder", "value",
                         "for", "rel", "target", "charset", "content", "autocomplete", "required",
                         "data-feedback-training", "viewBox", "d", "cx", "cy", "r",
                         "x1", "y1", "x2", "y2", "fill", "stroke", "stroke-width",
                         "stroke-linecap", "stroke-linejoin", "aria-label", "aria-hidden", "role"
                     })
            {
                _sanitizer.AllowedAttributes.Add(attr);
            }

            // Only complete tracking-link placeholders may bypass URI validation.
            // A URL containing a placeholder can still carry an executable scheme;
            // restoring it wholesale would reintroduce javascript:/data:/vbscript:.
            _sanitizer.FilterUrl += (_, e) =>
            {
                if (EmailLogoCatalog.Tokens.Any(token => string.Equals(e.OriginalUrl, $"cid:{token}", StringComparison.OrdinalIgnoreCase)))
                {
                    // Only embedded, fixed PNG logos can survive CID URI filtering.
                    e.SanitizedUrl = e.OriginalUrl;
                }
                else if (e.OriginalUrl?.StartsWith("cid:", StringComparison.OrdinalIgnoreCase) == true)
                {
                    e.SanitizedUrl = null;
                }
                else if (e.OriginalUrl is "{{LINK}}" or "{{LINK_PHISHING}}")
                {
                    e.SanitizedUrl = e.OriginalUrl;
                }
                else if (e.OriginalUrl?.Contains("{{", StringComparison.Ordinal) == true)
                {
                    e.SanitizedUrl = null;
                }
            };
            _sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is not IElement element || element.NamespaceUri != "http://www.w3.org/2000/svg") return;
                // SVG is restricted to static geometry. It cannot fetch/link another
                // document or restore executable URLs through SVG-specific attributes.
                element.RemoveAttribute("href");
                element.RemoveAttribute("xlink:href");
                foreach (var paint in new[] { "fill", "stroke" })
                    if (element.GetAttribute(paint)?.Contains("url(", StringComparison.OrdinalIgnoreCase) == true)
                        element.RemoveAttribute(paint);
            };
        }

        public string Sanitize(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return html ?? string.Empty;

            // Sem tags = ID de isca/landing oficial (ex.: "amazon-notificacao-seguranca")
            // ou texto puro. Não passa pelo sanitizador para não corromper o identificador.
            if (!html.Contains('<'))
                return html;

            return _sanitizer.Sanitize(html);
        }
    }
}
