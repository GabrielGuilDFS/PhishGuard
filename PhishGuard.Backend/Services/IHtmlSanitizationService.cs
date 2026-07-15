using Ganss.Xss;

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
                         "select", "option", "textarea", "link", "meta"
                     })
            {
                _sanitizer.AllowedTags.Add(tag);
            }

            foreach (var attr in new[]
                     {
                         "action", "method", "type", "name", "placeholder", "value",
                         "for", "rel", "target", "charset", "content", "autocomplete", "required"
                     })
            {
                _sanitizer.AllowedAttributes.Add(attr);
            }

            // Preserva placeholders de disparo ({{LINK}}, {{NOME}}, {{CAMPAIGN_ID}}...) em
            // href/src — não são URLs válidas e seriam descartadas pela validação de URI.
            _sanitizer.FilterUrl += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.OriginalUrl) && e.OriginalUrl.Contains("{{"))
                    e.SanitizedUrl = e.OriginalUrl;
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
