using System.Reflection;
using System.Text.RegularExpressions;

namespace PhishGuard.Backend.Content;

/// <summary>Official frontend catalogs embedded for immutable campaign snapshots.</summary>
public static partial class OfficialPageCatalog
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Landings =
        new(() => Load(".OfficialLandings.", ".html"));
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Education =
        new(() => Load(".EducationalTemplates.", ".html"));
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Feedback =
        new(() => Load(".OfficialFeedback.", ".json"));

    public static IEnumerable<string> LandingIds => Landings.Value.Keys;
    public static IEnumerable<string> FeedbackIds => Feedback.Value.Keys;
    public static IEnumerable<string> EducationalIds => Education.Value.Keys;

    public static string? ResolveLandingId(string? value)
    {
        var normalized = value == "mercadoliv-login" ? "mercado-liv-login" : value;
        return normalized is not null && Landings.Value.ContainsKey(normalized) ? normalized : null;
    }

    public static bool IsKnownLandingId(string? value) => ResolveLandingId(value) is not null;

    public static string ResolveLandingHtml(string value) =>
        ResolveLandingId(value) is { } id ? Landings.Value[id] : value;

    public static string RestoreLegacyLandingClasses(string html, string? templateId,
        Func<string, string> sanitize)
    {
        if (ResolveLandingId(templateId) is not { } id) return html;
        var official = sanitize(Landings.Value[id]);
        var withoutClasses = Regex.Replace(official, " class=\"[^\"]*\"", string.Empty);
        // Repair only the exact historical output of the class-stripping sanitizer.
        // Different text, styles or custom HTML keep their captured snapshot.
        return string.Equals(html, withoutClasses, StringComparison.Ordinal) ? official : html;
    }

    public static string ResolveEducationalHtml(string value) =>
        Education.Value.TryGetValue(value, out var html) ? html : value;

    public static string RestoreLegacyEducationalClasses(string html, Func<string, string> sanitize)
    {
        foreach (var source in Education.Value.Values)
        {
            var official = sanitize(source);
            var withoutClasses = Regex.Replace(official, " class=\"[^\"]*\"", string.Empty);
            if (string.Equals(html, withoutClasses, StringComparison.Ordinal)) return official;
        }
        return html;
    }

    public static string? ResolveFeedbackConfigJson(string htmlOrId)
    {
        var id = FeedbackMarker().Match(htmlOrId).Groups[1].Value;
        if (string.IsNullOrEmpty(id)) id = htmlOrId;
        return Feedback.Value.TryGetValue(id, out var json) ? json : null;
    }

    [GeneratedRegex("data-feedback-training=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase)]
    private static partial Regex FeedbackMarker();

    private static IReadOnlyDictionary<string, string> Load(string marker, string suffix)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var catalog = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var resource in assembly.GetManifestResourceNames())
        {
            var position = resource.IndexOf(marker, StringComparison.Ordinal);
            if (position < 0 || !resource.EndsWith(suffix, StringComparison.Ordinal)) continue;
            var start = position + marker.Length;
            var id = resource[start..^suffix.Length];
            using var stream = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Missing embedded catalog resource: {resource}");
            using var reader = new StreamReader(stream);
            catalog.Add(id, reader.ReadToEnd());
        }
        return catalog;
    }
}
