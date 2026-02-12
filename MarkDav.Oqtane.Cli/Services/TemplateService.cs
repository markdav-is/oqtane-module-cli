using System.Text.Json;
using MarkDav.Oqtane.Cli.Models;

namespace MarkDav.Oqtane.Cli.Services;

public class TemplateService : ITemplateService
{
    private static readonly string TemplatesRelativePath = Path.Combine("Oqtane.Server", "wwwroot", "Modules", "Templates");
    private const string ManifestFileName = "template.json";

    public string? FindSolutionDirectory(string startPath)
    {
        var directory = Directory.Exists(startPath)
            ? new DirectoryInfo(startPath)
            : new DirectoryInfo(Path.GetDirectoryName(startPath)!);

        while (directory is not null)
        {
            if (directory.EnumerateFiles("*.slnx").Any() ||
                directory.EnumerateFiles("*.sln").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    public string GetTemplatePath(string solutionDir, string templateType)
    {
        // Normalize template type to title case for directory name
        var dirName = templateType.ToLowerInvariant() switch
        {
            "internal" => "Internal",
            "external" => "External",
            _ => throw new ArgumentException(
                $"Template '{templateType}' not found. Use 'oqtane module list' to see available templates.")
        };

        return Path.Combine(solutionDir, TemplatesRelativePath, dirName);
    }

    public TemplateManifest LoadTemplateManifest(string templatePath)
    {
        var manifestPath = Path.Combine(templatePath, ManifestFileName);

        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException(
                $"Template manifest not found at '{manifestPath}'. " +
                "Ensure you are in an Oqtane solution directory with valid templates.");
        }

        var json = File.ReadAllText(manifestPath);

        TemplateManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<TemplateManifest>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid template manifest at '{manifestPath}': {ex.Message}", ex);
        }

        if (manifest is null)
        {
            throw new InvalidOperationException(
                $"Template manifest at '{manifestPath}' deserialized to null.");
        }

        return manifest;
    }

    public List<(string Type, TemplateManifest Manifest)> ListTemplates(string solutionDir)
    {
        var templatesDir = Path.Combine(solutionDir, TemplatesRelativePath);
        var results = new List<(string Type, TemplateManifest Manifest)>();

        if (!Directory.Exists(templatesDir))
        {
            return results;
        }

        foreach (var dir in Directory.GetDirectories(templatesDir))
        {
            var manifestPath = Path.Combine(dir, ManifestFileName);
            if (!File.Exists(manifestPath))
                continue;

            try
            {
                var manifest = LoadTemplateManifest(dir);
                var templateType = Path.GetFileName(dir).ToLowerInvariant();
                results.Add((templateType, manifest));
            }
            catch
            {
                // Skip templates with invalid manifests
            }
        }

        return results;
    }
}
