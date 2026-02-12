using FluentAssertions;
using MarkDav.Oqtane.Cli.Services;
using System.Text.RegularExpressions;
using Xunit;

namespace MarkDav.Oqtane.Cli.Tests.Services;

public class TokenReplacementServiceTests
{
    private readonly TokenReplacementService _service = new();

    [Fact]
    public void GenerateTokens_CreatesCorrectDictionary()
    {
        // Arrange
        var owner = "MarkDav";
        var moduleName = "WeatherArbitrage";
        var description = "Weather analysis module";
        var framework = "net10.0";

        // Act
        var tokens = _service.GenerateTokens(owner, moduleName, description, framework);

        // Assert
        tokens.Should().ContainKey("[Owner]").WhoseValue.Should().Be(owner);
        tokens.Should().ContainKey("[Module]").WhoseValue.Should().Be(moduleName);
        tokens.Should().ContainKey("[Description]").WhoseValue.Should().Be(description);
        tokens.Should().ContainKey("[Framework]").WhoseValue.Should().Be(framework);
        tokens.Should().ContainKey("[Year]").WhoseValue.Should().Be(DateTime.Now.Year.ToString());
        tokens.Should().ContainKey("[Date]");
    }

    [Fact]
    public void GenerateTokens_YearToken_IsCurrentYear()
    {
        // Arrange & Act
        var tokens = _service.GenerateTokens("Owner", "Module", "Description", "net10.0");

        // Assert
        tokens["[Year]"].Should().Be(DateTime.Now.Year.ToString());
    }

    [Fact]
    public void GenerateTokens_DateToken_IsInCorrectFormat()
    {
        // Arrange & Act
        var tokens = _service.GenerateTokens("Owner", "Module", "Description", "net10.0");

        // Assert
        tokens["[Date]"].Should().MatchRegex(@"\d{4}-\d{2}-\d{2}");
        tokens["[Date]"].Should().Be(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void ReplaceTokens_ReplacesStandardTokens()
    {
        // Arrange
        var content = "namespace [Owner].[Module] { }";
        var tokens = new Dictionary<string, string>
        {
            ["[Owner]"] = "Acme",
            ["[Module]"] = "Blog"
        };

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        result.Should().Be("namespace Acme.Blog { }");
    }

    [Fact]
    public void ReplaceTokens_WithMultipleTokens_ReplacesAll()
    {
        // Arrange
        var content = "[Owner].[Module] - [Description] ([Framework])";
        var tokens = new Dictionary<string, string>
        {
            ["[Owner]"] = "MarkDav",
            ["[Module]"] = "Weather",
            ["[Description]"] = "Weather module",
            ["[Framework]"] = "net10.0"
        };

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        result.Should().Be("MarkDav.Weather - Weather module (net10.0)");
    }

    [Fact]
    public void ReplaceTokens_WithGuidToken_GeneratesValidGuid()
    {
        // Arrange
        var content = "ID: [Guid]";
        var tokens = new Dictionary<string, string>();

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        result.Should().StartWith("ID: ");
        var guidPart = result.Substring(4);
        Guid.TryParse(guidPart, out _).Should().BeTrue();
    }

    [Fact]
    public void ReplaceTokens_WithMultipleGuidTokens_GeneratesUniqueGuids()
    {
        // Arrange
        var content = "[Guid] and [Guid] and [Guid]";
        var tokens = new Dictionary<string, string>();

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        var guidMatches = Regex.Matches(result, @"[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}");
        guidMatches.Should().HaveCount(3);
        
        var guids = guidMatches.Select(m => m.Value).ToList();
        guids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ReplaceTokens_WithEmptyContent_ReturnsEmpty()
    {
        // Arrange
        var content = "";
        var tokens = new Dictionary<string, string> { ["[Owner]"] = "Test" };

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTokens_WithNoTokensInContent_ReturnsUnchanged()
    {
        // Arrange
        var content = "No tokens here";
        var tokens = new Dictionary<string, string> { ["[Owner]"] = "Test" };

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        result.Should().Be("No tokens here");
    }

    [Fact]
    public void ReplaceTokens_IsCaseSensitive()
    {
        // Arrange
        var content = "[owner] and [Owner]";
        var tokens = new Dictionary<string, string> { ["[Owner]"] = "Acme" };

        // Act
        var result = _service.ReplaceTokens(content, tokens);

        // Assert
        result.Should().Be("[owner] and Acme");
    }

    [Fact]
    public void ReplaceTokensInPath_ReplacesStandardTokens()
    {
        // Arrange
        var path = "[Owner].[Module].Client/Controllers/[Module]Controller.cs";
        var tokens = new Dictionary<string, string>
        {
            ["[Owner]"] = "MarkDav",
            ["[Module]"] = "Weather"
        };

        // Act
        var result = _service.ReplaceTokensInPath(path, tokens);

        // Assert
        result.Should().Be("MarkDav.Weather.Client/Controllers/WeatherController.cs");
    }

    [Fact]
    public void ReplaceTokensInPath_DoesNotReplaceGuidToken()
    {
        // Arrange
        var path = "[Owner].[Module]/[Guid].txt";
        var tokens = new Dictionary<string, string>
        {
            ["[Owner]"] = "Acme",
            ["[Module]"] = "Blog"
        };

        // Act
        var result = _service.ReplaceTokensInPath(path, tokens);

        // Assert
        result.Should().Be("Acme.Blog/[Guid].txt");
        result.Should().Contain("[Guid]");
    }

    [Fact]
    public void ReplaceTokensInPath_WithEmptyPath_ReturnsEmpty()
    {
        // Arrange
        var path = "";
        var tokens = new Dictionary<string, string> { ["[Owner]"] = "Test" };

        // Act
        var result = _service.ReplaceTokensInPath(path, tokens);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTokensInPath_WithDirectoryPath_ReplacesCorrectly()
    {
        // Arrange
        var path = "Modules/[Owner].[Module]/";
        var tokens = new Dictionary<string, string>
        {
            ["[Owner]"] = "MarkDav",
            ["[Module]"] = "WeatherArbitrage"
        };

        // Act
        var result = _service.ReplaceTokensInPath(path, tokens);

        // Assert
        result.Should().Be("Modules/MarkDav.WeatherArbitrage/");
    }
}
