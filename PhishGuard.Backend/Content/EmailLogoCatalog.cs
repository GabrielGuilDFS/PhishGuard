using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PhishGuard.Backend.Content
{
    /// <summary>
    /// Catálogo das LOGOS de e-mail rasterizadas (PNG), embutidas em
    /// Resources/EmailAssets/*.png. São referenciadas nos corpos das iscas por
    /// <c>&lt;img src="cid:&lt;token&gt;"&gt;</c> e anexadas como recursos inline
    /// (Content-ID) no disparo (<see cref="Services.CampaignDispatchService"/>).
    ///
    /// Motivo: o Gmail NÃO renderiza SVG nem <c>data:</c> URIs em e-mail; imagens
    /// inline via CID, por outro lado, são exibidas por padrão (sem depender de
    /// hospedagem pública nem do clique em "exibir imagens").
    /// </summary>
    public static class EmailLogoCatalog
    {
        private const string ResourceMarker = ".EmailAssets.";

        // token de CID (usado como Content-ID e em "cid:<token>") -> nome do arquivo PNG.
        private static readonly IReadOnlyDictionary<string, string> _fileByToken =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["logo-bhomax"] = "bho-max-logo.png",
                ["logo-mercadoliv"] = "mercado-liv-logo.png",
            };

        // token -> bytes do PNG, carregados uma única vez dos recursos embutidos.
        private static readonly Lazy<IReadOnlyDictionary<string, byte[]>> _bytesByToken =
            new(LoadEmbeddedLogos);

        private static IReadOnlyDictionary<string, byte[]> LoadEmbeddedLogos()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var arquivoParaToken = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _fileByToken)
                arquivoParaToken[kv.Value] = kv.Key;

            var mapa = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var recurso in assembly.GetManifestResourceNames())
            {
                var marcador = recurso.IndexOf(ResourceMarker, StringComparison.Ordinal);
                if (marcador < 0) continue;

                var arquivo = recurso.Substring(marcador + ResourceMarker.Length);
                if (!arquivoParaToken.TryGetValue(arquivo, out var token)) continue;

                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null) continue;
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                mapa[token] = ms.ToArray();
            }

            return mapa;
        }

        /// <summary>Todos os tokens de CID conhecidos (para varrer o corpo do e-mail).</summary>
        public static IEnumerable<string> Tokens => _fileByToken.Keys;

        /// <summary>
        /// Resolve um token de CID para o PNG embutido. Retorna false se o token for
        /// desconhecido ou o recurso não tiver sido embutido.
        /// </summary>
        public static bool TryGetLogo(string token, out string fileName, out byte[] bytes)
        {
            fileName = string.Empty;
            bytes = Array.Empty<byte>();
            if (string.IsNullOrWhiteSpace(token)) return false;
            if (!_fileByToken.TryGetValue(token, out var nome)) return false;
            if (!_bytesByToken.Value.TryGetValue(token, out var data)) return false;
            fileName = nome;
            bytes = data;
            return true;
        }
    }
}
