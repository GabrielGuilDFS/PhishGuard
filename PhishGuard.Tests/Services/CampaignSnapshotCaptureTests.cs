using PhishGuard.Backend.Content;
using PhishGuard.Backend.Services;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace PhishGuard.Tests.Services;

public sealed class CampaignSnapshotCaptureTests
{
    [Fact]
    public void LegacyFeedbackRepair_RestoresClassesAndPreservesCapturedDatesAndCustomContent()
    {
        var sanitizer = new HtmlSanitizationService();
        var capturedAt = new DateTime(2026, 10, 5, 23, 31, 0, DateTimeKind.Utc);
        foreach (var id in OfficialPageCatalog.FeedbackIds)
        {
            var expected = CampaignSnapshotCapture.PrepareFeedbackJson(OfficialPageCatalog.ResolveFeedbackConfigJson(id), sanitizer, capturedAt)!;
            var node = JsonNode.Parse(expected)!;
            foreach (var mockup in node["mockups"]!.AsArray())
                mockup!["html"] = Regex.Replace(mockup["html"]!.GetValue<string>(), " class=\"[^\"]*\"", string.Empty);
            var broken = node.ToJsonString();
            Assert.Equal(expected, CampaignSnapshotCapture.RestoreLegacyFeedbackClasses(broken, sanitizer, capturedAt));
            node["header"]!["titulo"] = "Título personalizado da campanha";
            var custom = node.ToJsonString();
            Assert.Equal(custom, CampaignSnapshotCapture.RestoreLegacyFeedbackClasses(custom, sanitizer, capturedAt));
            Assert.Equal(expected, CampaignSnapshotCapture.RestoreLegacyFeedbackClasses(expected, sanitizer, capturedAt));
        }
        Assert.Null(CampaignSnapshotCapture.RestoreLegacyFeedbackClasses(null, sanitizer, capturedAt));
    }
}
