using FluentAssertions;
using MarkDav.Oqtane.Cli.Services;
using System.Text.Json;
using Xunit;

namespace MarkDav.Oqtane.Cli.Tests.Services;

public class TemplateServiceTests : IDisposable
{
    private readonly TemplateService _service = new();
    private readonly string _testDirectory;

    public TemplateServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"OqtaneCliTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void FindSolutionDirectory_WithSlnxFile_ReturnsSolutionDirectory()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "MySolution");
        Directory.CreateDirectory(solutionDir);
        File.WriteAllText(Path.Combine(solutionDir, "Oqtane.slnx"), "{}");

        // Act
        var result = _service.FindSolutionDirectory(solutionDir);

        // Assert
        result.Should().Be(solutionDir);
    }

    [Fact]
    public void FindSolutionDirectory_WithSlnFile_ReturnsSolutionDirectory()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "MySolution");
        Directory.CreateDirectory(solutionDir);
        File.WriteAllText(Path.Combine(solutionDir, "Oqtane.sln"), "");

        // Act
        var result = _service.FindSolutionDirectory(solutionDir);

        // Assert
        result.Should().Be(solutionDir);
    }

    [Fact]
    public void FindSolutionDirectory_WalksUpDirectoryTree()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "MySolution");
        var subDir = Path.Combine(solutionDir, "SubDir1", "SubDir2");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(solutionDir, "Oqtane.slnx"), "{}");

        // Act
        var result = _service.FindSolutionDirectory(subDir);

        // Assert
        result.Should().Be(solutionDir);
    }

    [Fact]
    public void FindSolutionDirectory_NoSolutionFound_ReturnsNull()
    {
        // Arrange
        var dir = Path.Combine(_testDirectory, "NoSolution");
        Directory.CreateDirectory(dir);

        // Act
        var result = _service.FindSolutionDirectory(dir);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindSolutionDirectory_WithFilePath_SearchesFromParentDirectory()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "MySolution");
        Directory.CreateDirectory(solutionDir);
        File.WriteAllText(Path.Combine(solutionDir, "Oqtane.sln"), "");
        var filePath = Path.Combine(solutionDir, "somefile.txt");

        // Act
        var result = _service.FindSolutionDirectory(filePath);

        // Assert
        result.Should().Be(solutionDir);
    }

    [Fact]
    public void GetTemplatePath_WithInternal_ReturnsInternalTemplatePath()
    {
        // Arrange
        var solutionDir = Path.Combine(Path.GetTempPath(), "solution");

        // Act
        var result = _service.GetTemplatePath(solutionDir, "internal");

        // Assert
        result.Should().Be(Path.Combine(solutionDir, "Oqtane.Server", "wwwroot", "Modules", "Templates", "Internal"));
    }

    [Fact]
    public void GetTemplatePath_WithExternal_ReturnsExternalTemplatePath()
    {
        // Arrange
        var solutionDir = Path.Combine(Path.GetTempPath(), "solution");

        // Act
        var result = _service.GetTemplatePath(solutionDir, "external");

        // Assert
        result.Should().Be(Path.Combine(solutionDir, "Oqtane.Server", "wwwroot", "Modules", "Templates", "External"));
    }

    [Fact]
    public void GetTemplatePath_CaseInsensitive()
    {
        // Arrange
        var solutionDir = Path.Combine(Path.GetTempPath(), "solution");

        // Act
        var resultLower = _service.GetTemplatePath(solutionDir, "internal");
        var resultUpper = _service.GetTemplatePath(solutionDir, "INTERNAL");
        var resultMixed = _service.GetTemplatePath(solutionDir, "InTeRnAl");

        // Assert
        resultLower.Should().Be(resultUpper);
        resultLower.Should().Be(resultMixed);
    }

    [Fact]
    public void GetTemplatePath_WithInvalidTemplate_ThrowsArgumentException()
    {
        // Arrange
        var solutionDir = Path.Combine(Path.GetTempPath(), "solution");

        // Act
        Action act = () => _service.GetTemplatePath(solutionDir, "invalid");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void LoadTemplateManifest_WithValidManifest_ReturnsManifest()
    {
        // Arrange
        var templateDir = Path.Combine(_testDirectory, "Template");
        Directory.CreateDirectory(templateDir);
        
        var manifest = new
        {
            name = "Test Template",
            version = "1.0.0",
            owner = "TestOwner",
            module = "TestModule",
            description = "Test Description",
            files = new[]
            {
                new { path = "[Owner].[Module].Client/Module.cs", tokens = new[] { "[Owner]", "[Module]" } }
            }
        };
        
        File.WriteAllText(
            Path.Combine(templateDir, "template.json"),
            JsonSerializer.Serialize(manifest));

        // Act
        var result = _service.LoadTemplateManifest(templateDir);

        // Assert
        result.Name.Should().Be("Test Template");
        result.Version.Should().Be("1.0.0");
        result.Owner.Should().Be("TestOwner");
        result.Module.Should().Be("TestModule");
        result.Description.Should().Be("Test Description");
        result.Files.Should().HaveCount(1);
        result.Files[0].Path.Should().Be("[Owner].[Module].Client/Module.cs");
        result.Files[0].Tokens.Should().Contain("[Owner]").And.Contain("[Module]");
    }

    [Fact]
    public void LoadTemplateManifest_ManifestNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var templateDir = Path.Combine(_testDirectory, "NoManifest");
        Directory.CreateDirectory(templateDir);

        // Act
        Action act = () => _service.LoadTemplateManifest(templateDir);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void LoadTemplateManifest_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var templateDir = Path.Combine(_testDirectory, "InvalidJson");
        Directory.CreateDirectory(templateDir);
        File.WriteAllText(Path.Combine(templateDir, "template.json"), "{ invalid json");

        // Act
        Action act = () => _service.LoadTemplateManifest(templateDir);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid template manifest*");
    }

    [Fact]
    public void ListTemplates_WithMultipleTemplates_ReturnsAll()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "Solution");
        var templatesDir = Path.Combine(solutionDir, "Oqtane.Server", "wwwroot", "Modules", "Templates");
        
        var internalDir = Path.Combine(templatesDir, "Internal");
        var externalDir = Path.Combine(templatesDir, "External");
        Directory.CreateDirectory(internalDir);
        Directory.CreateDirectory(externalDir);

        var internalManifest = new
        {
            name = "Internal Template",
            version = "1.0.0",
            owner = "",
            module = "",
            description = "",
            files = Array.Empty<object>()
        };
        
        var externalManifest = new
        {
            name = "External Template",
            version = "1.0.0",
            owner = "",
            module = "",
            description = "",
            files = Array.Empty<object>()
        };

        File.WriteAllText(
            Path.Combine(internalDir, "template.json"),
            JsonSerializer.Serialize(internalManifest));
        
        File.WriteAllText(
            Path.Combine(externalDir, "template.json"),
            JsonSerializer.Serialize(externalManifest));

        // Act
        var result = _service.ListTemplates(solutionDir);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Type == "internal" && t.Manifest.Name == "Internal Template");
        result.Should().Contain(t => t.Type == "external" && t.Manifest.Name == "External Template");
    }

    [Fact]
    public void ListTemplates_SkipsTemplatesWithInvalidManifests()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "Solution");
        var templatesDir = Path.Combine(solutionDir, "Oqtane.Server", "wwwroot", "Modules", "Templates");
        
        var internalDir = Path.Combine(templatesDir, "Internal");
        var externalDir = Path.Combine(templatesDir, "External");
        Directory.CreateDirectory(internalDir);
        Directory.CreateDirectory(externalDir);

        // Valid external manifest
        var externalManifest = new
        {
            name = "External Template",
            version = "1.0.0",
            owner = "",
            module = "",
            description = "",
            files = Array.Empty<object>()
        };

        File.WriteAllText(Path.Combine(internalDir, "template.json"), "{ invalid }");
        File.WriteAllText(
            Path.Combine(externalDir, "template.json"),
            JsonSerializer.Serialize(externalManifest));

        // Act
        var result = _service.ListTemplates(solutionDir);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(t => t.Type == "external");
        result.Should().NotContain(t => t.Type == "internal");
    }

    [Fact]
    public void ListTemplates_NoTemplatesDirectory_ReturnsEmpty()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "EmptySolution");
        Directory.CreateDirectory(solutionDir);

        // Act
        var result = _service.ListTemplates(solutionDir);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ListTemplates_SkipsDirectoriesWithoutManifest()
    {
        // Arrange
        var solutionDir = Path.Combine(_testDirectory, "Solution");
        var templatesDir = Path.Combine(solutionDir, "Oqtane.Server", "wwwroot", "Modules", "Templates");
        
        var internalDir = Path.Combine(templatesDir, "Internal");
        var noManifestDir = Path.Combine(templatesDir, "NoManifest");
        Directory.CreateDirectory(internalDir);
        Directory.CreateDirectory(noManifestDir);

        var manifest = new
        {
            name = "Internal Template",
            version = "1.0.0",
            owner = "",
            module = "",
            description = "",
            files = Array.Empty<object>()
        };

        File.WriteAllText(
            Path.Combine(internalDir, "template.json"),
            JsonSerializer.Serialize(manifest));

        // Act
        var result = _service.ListTemplates(solutionDir);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(t => t.Type == "internal");
    }
}
