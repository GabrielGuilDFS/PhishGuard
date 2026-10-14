using Microsoft.EntityFrameworkCore;
using PhishGuard.Backend.Content;
using PhishGuard.Backend.Data;
using PhishGuard.Backend.Models;
using PhishGuard.Backend.Services.Delivery;
using PhishGuard.Backend.Utilities;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace PhishGuard.Backend.Services;

// Called inside the tenant mutation transaction before a draft leaves that state.
// Once captured, retries never consult mutable catalogs or target contact data.
internal static class CampaignSnapshotCapture
{
    internal static async Task CaptureAsync(AppDbContext context, Campaign campaign,
        SmtpConfig deliveryConfig, IHtmlSanitizationService sanitizer,
        CancellationToken cancellationToken)
    {
        if (campaign.SnapshotCapturedAtUtc.HasValue) return;

        var tenantId = campaign.TenantId;
        var template = await context.Templates.FirstOrDefaultAsync(t => t.Id == campaign.EmailTemplateId && t.TenantId == tenantId, cancellationToken);
        var landing = await context.PhishingPages.FirstOrDefaultAsync(p => p.Id == campaign.LandingPageId && p.TenantId == tenantId, cancellationToken);
        var education = await context.EducationalPages.FirstOrDefaultAsync(p => p.Id == campaign.EducationalPageId && p.TenantId == tenantId, cancellationToken);
        var participants = await context.CampaignTargets.Include(p => p.Target)
            .Where(p => p.CampaignsId == campaign.Id && p.TenantId == tenantId).ToListAsync(cancellationToken);
        if (template is null || landing is null || education is null || participants.Count == 0
            || participants.Any(p => p.Target.TenantId != tenantId))
            throw new InvalidOperationException("Os recursos da campanha não pertencem ao mesmo tenant.");

        var capturedAt = DateTime.UtcNow;
        campaign.TemplateNameSnapshot = template.Nome;
        campaign.EmailSubjectSnapshot = template.Assunto;
        campaign.EmailBodySnapshot = sanitizer.Sanitize(OfficialBaitCatalog.ResolveHtml(template.CorpoHtml));
        campaign.SenderNameSnapshot = string.IsNullOrWhiteSpace(deliveryConfig.SenderName) ? template.RemetenteNome : deliveryConfig.SenderName;
        campaign.SenderEmailSnapshot = deliveryConfig.ProviderType == EmailProviderType.Smtp ? deliveryConfig.Usuario : deliveryConfig.SenderEmail;
        campaign.LandingNameSnapshot = landing.Nome;
        campaign.LandingTemplateIdSnapshot = OfficialPageCatalog.ResolveLandingId(landing.HtmlCaptura);
        campaign.LandingHtmlSnapshot = sanitizer.Sanitize(OfficialPageCatalog.ResolveLandingHtml(landing.HtmlCaptura));
        campaign.EducationalNameSnapshot = education.Nome;
        campaign.EducationalHtmlSnapshot = sanitizer.Sanitize(OfficialPageCatalog.ResolveEducationalHtml(education.HtmlEducacional));
        campaign.EducationalFeedbackConfigJsonSnapshot = PrepareFeedbackJson(
            OfficialPageCatalog.ResolveFeedbackConfigJson(education.HtmlEducacional), sanitizer, capturedAt);
        campaign.SnapshotCapturedAtUtc = capturedAt;
        campaign.IsLegacySnapshot = false;
        foreach (var participant in participants)
        {
            participant.TargetNameSnapshot = participant.Target.Nome;
            participant.TargetEmailSnapshot = participant.Target.Email;
            participant.DepartmentSnapshot = participant.Target.Departamento;
            participant.SnapshotCapturedAtUtc = capturedAt;
            participant.IsLegacySnapshot = false;
        }
    }

    internal static string? RestoreLegacyFeedbackClasses(string? json, IHtmlSanitizationService sanitizer, DateTime timestampUtc)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        var captured = JsonNode.Parse(json);
        var id = captured?["id"]?.GetValue<string>();
        if (id is null) return json;
        var expectedJson = PrepareFeedbackJson(OfficialPageCatalog.ResolveFeedbackConfigJson(id), sanitizer, timestampUtc);
        if (expectedJson is null) return json;
        var withoutClasses = JsonNode.Parse(expectedJson);
        StripClasses(withoutClasses);
        // Preserve all captured content and dates unless only CSS classes were lost.
        return JsonNode.DeepEquals(captured, withoutClasses) ? expectedJson : json;

        static void StripClasses(JsonNode? node)
        {
            if (node is JsonArray array)
                foreach (var child in array) StripClasses(child);
            else if (node is JsonObject obj)
                foreach (var property in obj.ToArray())
                    if (property.Key == "html" && property.Value is JsonValue value && value.TryGetValue<string>(out var html))
                        obj[property.Key] = Regex.Replace(html, " class=\"[^\"]*\"", string.Empty);
                    else StripClasses(property.Value);
        }
    }

    internal static string? PrepareFeedbackJson(string? json, IHtmlSanitizationService sanitizer, DateTime timestampUtc)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        var timestamp = HorarioBrasilia.Converter(timestampUtc);
        var expiry = timestamp.AddHours(2);
        var expiryLabel = expiry.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"))
            + " às " + expiry.ToString("hh:mm tt", CultureInfo.GetCultureInfo("en-US"));
        var accessLabel = timestamp.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            + " às " + timestamp.ToString("HH:mm", CultureInfo.InvariantCulture) + " (BRT)";
        var node = JsonNode.Parse(json);
        SanitizeNode(node);
        return node?.ToJsonString();

        void SanitizeNode(JsonNode? item)
        {
            if (item is JsonArray array)
            {
                foreach (var child in array) SanitizeNode(child);
            }
            else if (item is JsonObject obj)
            {
                foreach (var property in obj.ToArray())
                {
                    if (property.Key.Equals("html", StringComparison.OrdinalIgnoreCase)
                        && property.Value is JsonValue value && value.TryGetValue<string>(out var html))
                    {
                        obj[property.Key] = sanitizer.Sanitize(html
                            .Replace("{{DATA_EXPIRACAO}}", expiryLabel)
                            .Replace("{{DATA_ACESSO}}", accessLabel));
                    }
                    else SanitizeNode(property.Value);
                }
            }
        }
    }
}
