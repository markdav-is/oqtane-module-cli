using System.Text.Json.Serialization;

namespace MarkDav.Oqtane.Cli.Models;

public class TemplateFile
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("tokens")]
    public List<string> Tokens { get; set; } = [];
}
