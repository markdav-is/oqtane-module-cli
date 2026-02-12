using System.Text.Json.Serialization;

namespace MarkDav.Oqtane.Cli.Models;

public class TemplateManifest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("module")]
    public string Module { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<TemplateFile> Files { get; set; } = [];
}
