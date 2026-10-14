using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PhishGuard.Backend.Configuration;

public static class TrackingUrlConfiguration
{
    public static void Apply(ConfigurationManager configuration, IHostEnvironment environment)
    {
        var mode = configuration["AppSettings:TrackingUrlMode"]?.Trim();
        var useLocalUrls = mode?.ToLowerInvariant() switch
        {
            null or "" or "auto" => environment.IsDevelopment(),
            "local" => true,
            "public" => false,
            _ => throw new InvalidOperationException(
                "AppSettings:TrackingUrlMode deve ser Auto, Local ou Public.")
        };

        // Workers não possuem HttpContext. Resolva uma única vez no startup,
        // sem derivar links de Host ou X-Forwarded-* fornecidos por requisições.
        var apiKey = useLocalUrls ? "LocalApiBaseUrl" : "PublicApiBaseUrl";
        var appKey = useLocalUrls ? "LocalAppBaseUrl" : "PublicAppBaseUrl";
        var apiUrl = ValidateUrl(configuration[$"AppSettings:{apiKey}"], apiKey,
            useLocalUrls, "http://localhost:5173");
        var appUrl = ValidateUrl(configuration[$"AppSettings:{appKey}"], appKey,
            useLocalUrls, "http://localhost:5173");

        // Todos os consumidores existentes (compositor, redirects e CORS) recebem
        // as mesmas URLs efetivas, inclusive quando há um domínio antigo no .env.
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AppSettings:PublicApiBaseUrl"] = apiUrl,
            ["AppSettings:PublicAppBaseUrl"] = appUrl
        });
    }

    private static string ValidateUrl(string? value, string key, bool local, string localDefault)
    {
        var url = string.IsNullOrWhiteSpace(value) && local ? localDefault : value?.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)
            || uri.AbsolutePath != "/"
            || uri.IsLoopback != local)
        {
            var requiredHost = local ? "local (localhost ou loopback)" : "público";
            throw new InvalidOperationException(
                $"AppSettings:{key} deve ser uma origem HTTP/HTTPS com host {requiredHost}, " +
                "sem credenciais, caminho, query ou fragmento.");
        }

        return uri.GetLeftPart(UriPartial.Authority);
    }
}
