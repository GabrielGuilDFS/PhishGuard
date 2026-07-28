using System;
using System.Collections.Generic;

namespace PhishGuard.Backend.Contracts
{
    /// <summary>
    /// Contrato de rastreamento COMPARTILHADO (Passo 12 do roadmap de QA) — espelho
    /// C# do módulo TS <c>PhishGuard.Frontend/src/shared/trackingContract.ts</c>.
    ///
    /// Único ponto de verdade, no backend, sobre os nomes de parâmetro e o formato
    /// do <c>redirectUrl</c> da tela educacional. Existe para eliminar a divergência
    /// histórica (§1.3d): o front redireciona para
    /// <c>/educational-feedback?template=…&amp;c=…&amp;t=…</c> e o back devolvia
    /// <c>redirectUrl = /educational-feedback?campaign=…</c> — nomes incompatíveis.
    /// Agora ambos usam os MESMOS nomes canônicos (<c>c</c>/<c>t</c>).
    ///
    /// Ao alterar aqui, altere também o módulo TS — e vice-versa. Os dois lados são
    /// testados contra este mesmo formato.
    /// </summary>
    public static class TrackingContract
    {
        /// <summary>Rota (SPA) da tela educacional de feedback.</summary>
        public const string EducationalFeedbackPath = "/educational-feedback";

        /// <summary>Parâmetro: qual treinamento renderizar (opcional no redirect do back).</summary>
        public const string QueryTemplate = "template";

        /// <summary>Parâmetro canônico do ID da campanha (era o divergente "campaign").</summary>
        public const string QueryCampaign = "c";

        /// <summary>Parâmetro canônico do ID do alvo.</summary>
        public const string QueryTarget = "t";

        /// <summary>
        /// Monta a URL canônica da tela educacional de feedback (espelho de
        /// <c>educationalFeedbackUrl</c> no TS). Campos nulos/vazios são omitidos.
        /// </summary>
        public static string EducationalFeedbackUrl(string? template, string? campaignId, string? targetId)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(template))
                parts.Add($"{QueryTemplate}={Uri.EscapeDataString(template)}");
            if (!string.IsNullOrEmpty(campaignId))
                parts.Add($"{QueryCampaign}={Uri.EscapeDataString(campaignId)}");
            if (!string.IsNullOrEmpty(targetId))
                parts.Add($"{QueryTarget}={Uri.EscapeDataString(targetId)}");

            return parts.Count == 0
                ? EducationalFeedbackPath
                : $"{EducationalFeedbackPath}?{string.Join("&", parts)}";
        }
    }
}
