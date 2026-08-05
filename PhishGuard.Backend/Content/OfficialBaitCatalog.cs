using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PhishGuard.Backend.Content
{
    /// <summary>
    /// Catálogo das iscas oficiais homologadas (bho MAX, NetsFlix, amzprime, ...).
    ///
    /// A tela de Templates passou a persistir APENAS o identificador da isca
    /// (ex.: "amazon-notificacao-geral") no campo CorpoHtml — nunca mais a string
    /// massiva de HTML. Este catálogo resolve esse identificador de volta para o
    /// HTML real no momento do disparo (CampaignsController.IniciarDisparo).
    ///
    /// Os corpos HTML vivem como recursos EMBUTIDOS em Resources/OfficialBaits/*.html,
    /// gerados a partir do catálogo do frontend (predefinedTemplates.ts) para evitar
    /// divergência/transcrição manual.
    /// </summary>
    public static class OfficialBaitCatalog
    {
        private const string ResourceMarker = ".OfficialBaits.";
        private const string ResourceSuffix = ".html";

        // Mapa id -> HTML, carregado uma única vez a partir dos recursos embutidos.
        private static readonly Lazy<IReadOnlyDictionary<string, string>> _baits =
            new(LoadEmbeddedBaits);

        private static IReadOnlyDictionary<string, string> LoadEmbeddedBaits()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var recurso in assembly.GetManifestResourceNames())
            {
                var marcador = recurso.IndexOf(ResourceMarker, StringComparison.Ordinal);
                if (marcador < 0 || !recurso.EndsWith(ResourceSuffix, StringComparison.Ordinal))
                    continue;

                var inicio = marcador + ResourceMarker.Length;
                var id = recurso.Substring(inicio, recurso.Length - inicio - ResourceSuffix.Length);

                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null) continue;
                using var reader = new StreamReader(stream);
                mapa[id] = reader.ReadToEnd();
            }

            return mapa;
        }

        /// <summary>Identificadores de iscas oficiais disponíveis.</summary>
        public static IEnumerable<string> Ids => _baits.Value.Keys;

        /// <summary>True se o valor informado é um identificador de isca oficial conhecido.</summary>
        public static bool IsKnownId(string value) =>
            !string.IsNullOrWhiteSpace(value) && _baits.Value.ContainsKey(value);

        /// <summary>
        /// Resolve o valor persistido em Template.CorpoHtml para o HTML de envio.
        /// Se for o identificador de uma isca oficial, retorna o HTML do catálogo;
        /// caso contrário devolve o próprio valor (compatibilidade com registros
        /// legados que ainda guardam HTML bruto).
        /// </summary>
        public static string ResolveHtml(string corpoHtmlOuId)
        {
            if (!string.IsNullOrWhiteSpace(corpoHtmlOuId) &&
                _baits.Value.TryGetValue(corpoHtmlOuId, out var html))
            {
                return html;
            }

            return corpoHtmlOuId;
        }
    }
}
