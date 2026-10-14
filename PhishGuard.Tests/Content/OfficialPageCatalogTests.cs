using PhishGuard.Backend.Content;
using System.Text.Json;
using System.Text.RegularExpressions;
using PhishGuard.Backend.Services;

namespace PhishGuard.Tests.Content;

public sealed class OfficialPageCatalogTests
{
    [Fact]
    public void EducationalClassRepair_PreservesLayoutWithoutReplacingCustomContent()
    {
        var sanitizer = new HtmlSanitizationService();
        foreach (var id in OfficialPageCatalog.EducationalIds)
        {
            var official = sanitizer.Sanitize(OfficialPageCatalog.ResolveEducationalHtml(id));
            Assert.Contains("class=", official);
            var broken = Regex.Replace(official, " class=\"[^\"]*\"", string.Empty);
            Assert.Equal(official, OfficialPageCatalog.RestoreLegacyEducationalClasses(broken, sanitizer.Sanitize));
            Assert.Equal(broken + "<p>Custom</p>", OfficialPageCatalog.RestoreLegacyEducationalClasses(broken + "<p>Custom</p>", sanitizer.Sanitize));
            Assert.Equal(id, OfficialPageCatalog.RestoreLegacyEducationalClasses(id, sanitizer.Sanitize));
        }
    }

    [Fact]
    public void LegacyClassRepair_ChangesOnlyMatchingOfficialMarkup()
    {
        var sanitizer = new HtmlSanitizationService();
        foreach (var id in OfficialPageCatalog.LandingIds)
        {
            var official = sanitizer.Sanitize(OfficialPageCatalog.ResolveLandingHtml(id));
            var broken = Regex.Replace(official, " class=\"[^\"]*\"", string.Empty);
            Assert.Equal(official, OfficialPageCatalog.RestoreLegacyLandingClasses(broken, id, sanitizer.Sanitize));
            Assert.Equal(broken + "<p>Custom</p>", OfficialPageCatalog.RestoreLegacyLandingClasses(broken + "<p>Custom</p>", id, sanitizer.Sanitize));
            Assert.Equal(broken, OfficialPageCatalog.RestoreLegacyLandingClasses(broken, null, sanitizer.Sanitize));
            Assert.Equal(official, OfficialPageCatalog.RestoreLegacyLandingClasses(official, id, sanitizer.Sanitize));
        }
    }

    [Fact]
    public void SanitizedOfficialLandings_PreserveClassesAcrossSnapshotAndResponse()
    {
        var sanitizer = new HtmlSanitizationService();
        foreach (var id in OfficialPageCatalog.LandingIds)
        {
            var source = OfficialPageCatalog.ResolveLandingHtml(id);
            var snapshot = sanitizer.Sanitize(source);
            var response = sanitizer.Sanitize(snapshot);
            var classes = Regex.Matches(source, "class=\"([^\"]+)\"")
                .Select(match => match.Groups[1].Value).Distinct().ToArray();
            Assert.NotEmpty(classes);
            foreach (var value in classes)
                Assert.Contains($"class=\"{value}\"", response);
        }
    }

    [Fact]
    public void LandingResources_AreHtmlAndPreserveHistoricalAlias()
    {
        Assert.Equal(5, OfficialPageCatalog.LandingIds.Count());
        foreach (var id in OfficialPageCatalog.LandingIds)
            Assert.Contains("<form", OfficialPageCatalog.ResolveLandingHtml(id));
        Assert.Equal("mercado-liv-login", OfficialPageCatalog.ResolveLandingId("mercadoliv-login"));
        Assert.Equal(OfficialPageCatalog.ResolveLandingHtml("mercado-liv-login"), OfficialPageCatalog.ResolveLandingHtml("mercadoliv-login"));
        Assert.Equal("<p>Custom</p>", OfficialPageCatalog.ResolveLandingHtml("<p>Custom</p>"));
    }

    [Fact]
    public void FeedbackAndEducationalResources_AreCompleteAndParseable()
    {
        Assert.Equal(5, OfficialPageCatalog.FeedbackIds.Count());
        foreach (var id in OfficialPageCatalog.FeedbackIds)
        {
            var marker = $"<div data-feedback-training=\"{id}\"></div>";
            using var json = JsonDocument.Parse(OfficialPageCatalog.ResolveFeedbackConfigJson(marker)!);
            Assert.Equal(id, json.RootElement.GetProperty("id").GetString());
            Assert.NotEmpty(json.RootElement.GetProperty("mockups").EnumerateArray());
        }
        Assert.Equal(2, OfficialPageCatalog.EducationalIds.Count());
        foreach (var id in OfficialPageCatalog.EducationalIds)
            Assert.Contains("<html", OfficialPageCatalog.ResolveEducationalHtml(id));
        Assert.Null(OfficialPageCatalog.ResolveFeedbackConfigJson("<p>Custom</p>"));
    }

    [Fact]
    public void SanitizedOfficialLandings_PreserveEveryStaticSvgIcon()
    {
        var sanitizer = new HtmlSanitizationService();
        var originalTotal = 0;
        foreach (var id in OfficialPageCatalog.LandingIds)
        {
            var source = OfficialPageCatalog.ResolveLandingHtml(id);
            var originalCount = Regex.Matches(source, @"<svg\b", RegexOptions.IgnoreCase).Count;
            originalTotal += originalCount;
            Assert.Equal(originalCount, Regex.Matches(sanitizer.Sanitize(source), @"<svg\b", RegexOptions.IgnoreCase).Count);
        }
        Assert.True(originalTotal > 0);
    }
}
