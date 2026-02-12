using System.Text;

namespace MarkDav.Oqtane.Cli.Tests.Helpers;

/// <summary>
/// Helper class for running CLI commands and capturing output
/// </summary>
public class CliTestHelper : IDisposable
{
    private readonly string _testDirectory;
    private readonly StringWriter _outputWriter;
    private readonly StringWriter _errorWriter;
    private readonly TextWriter _originalOutput;
    private readonly TextWriter _originalError;

    public string Output => _outputWriter.ToString();
    public string Error => _errorWriter.ToString();
    public string TestDirectory => _testDirectory;

    public CliTestHelper()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"OqtaneCliTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _outputWriter = new StringWriter();
        _errorWriter = new StringWriter();
        _originalOutput = Console.Out;
        _originalError = Console.Error;
    }

    /// <summary>
    /// Create a mock Oqtane solution structure for testing
    /// </summary>
    public void CreateMockOqtaneSolution(bool includeTemplates = true)
    {
        // Create solution file
        File.WriteAllText(Path.Combine(_testDirectory, "Oqtane.slnx"), "{}");

        if (includeTemplates)
        {
            CreateTemplateDirectory("Internal", "Internal Module Template");
            CreateTemplateDirectory("External", "External Module Template");
        }
    }

    /// <summary>
    /// Create a template directory with manifest
    /// </summary>
    public void CreateTemplateDirectory(string templateType, string templateName, bool validManifest = true)
    {
        var templateDir = Path.Combine(
            _testDirectory,
            "Oqtane.Server",
            "wwwroot",
            "Modules",
            "Templates",
            templateType);

        Directory.CreateDirectory(templateDir);

        if (validManifest)
        {
            var manifestContent = $$"""
            {
              "name": "{{templateName}}",
              "version": "1.0.0",
              "owner": "Default",
              "module": "Template",
              "description": "{{templateType}} module template",
              "files": [
                {
                  "path": "[Owner].[Module].Client/Module.cs",
                  "tokens": ["[Owner]", "[Module]", "[Description]"]
                },
                {
                  "path": "[Owner].[Module].Server/Controllers/[Module]Controller.cs",
                  "tokens": ["[Owner]", "[Module]", "[Guid]"]
                }
              ]
            }
            """;

            File.WriteAllText(Path.Combine(templateDir, "template.json"), manifestContent);
        }
        else
        {
            File.WriteAllText(Path.Combine(templateDir, "template.json"), "{ invalid json");
        }
    }

    /// <summary>
    /// Create a subdirectory within the test directory
    /// </summary>
    public string CreateSubdirectory(string path)
    {
        var fullPath = Path.Combine(_testDirectory, path);
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Run a CLI command and capture output
    /// </summary>
    public int RunCommand(params string[] args)
    {
        // Redirect console output
        Console.SetOut(_outputWriter);
        Console.SetError(_errorWriter);

        try
        {
            // This would call your actual CLI entry point
            // For now, we'll return a placeholder
            // In a real scenario, you'd call: return Program.Main(args);
            throw new NotImplementedException(
                "CLI execution needs to be refactored to support testing. " +
                "Consider extracting command logic into testable methods.");
        }
        finally
        {
            // Restore original console output
            Console.SetOut(_originalOutput);
            Console.SetError(_originalError);
        }
    }

    public void Dispose()
    {
        _outputWriter.Dispose();
        _errorWriter.Dispose();
        Console.SetOut(_originalOutput);
        Console.SetError(_originalError);

        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
