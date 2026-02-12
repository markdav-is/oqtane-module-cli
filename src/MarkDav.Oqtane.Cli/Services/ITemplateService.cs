using MarkDav.Oqtane.Cli.Models;

namespace MarkDav.Oqtane.Cli.Services;

public interface ITemplateService
{
    /// <summary>
    /// Walk up directories from <paramref name="startPath"/> looking for a .slnx or .sln file.
    /// Returns the directory containing the solution, or null if not found.
    /// </summary>
    string? FindSolutionDirectory(string startPath);

    /// <summary>
    /// Resolve the template directory path for a given template type (e.g. "internal" or "external").
    /// Path: {solutionDir}/Oqtane.Server/wwwroot/Modules/Templates/{Internal|External}/
    /// </summary>
    string GetTemplatePath(string solutionDir, string templateType);

    /// <summary>
    /// Deserialize template.json from the given template directory.
    /// </summary>
    TemplateManifest LoadTemplateManifest(string templatePath);

    /// <summary>
    /// Enumerate all available templates in the solution's template directories.
    /// Returns a list of (templateType, manifest) pairs.
    /// </summary>
    List<(string Type, TemplateManifest Manifest)> ListTemplates(string solutionDir);
}
