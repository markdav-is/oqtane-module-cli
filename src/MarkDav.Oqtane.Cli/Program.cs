using System.CommandLine;
using MarkDav.Oqtane.Cli.Services;

var templateService = new TemplateService();

var rootCommand = new RootCommand("Oqtane CLI - Module and theme scaffolding tool");

// ── module command ──────────────────────────────────────────────────────────
var moduleCommand = new Command("module", "Create and manage Oqtane modules");
rootCommand.Subcommands.Add(moduleCommand);

// ── module create ───────────────────────────────────────────────────────────
var createCommand = new Command("create", "Create a new Oqtane module");
moduleCommand.Subcommands.Add(createCommand);

var ownerOption = new Option<string>("--owner") { Description = "Owner/company name for namespace", Required = true };
var nameOption = new Option<string>("--name") { Description = "Module name", Required = true };
var templateOption = new Option<string>("--template") { Description = "Template type: internal or external", DefaultValueFactory = _ => "internal" };
var descriptionOption = new Option<string>("--description") { Description = "Brief module description", DefaultValueFactory = _ => "" };
var solutionOption = new Option<string>("--solution") { Description = "Path to Oqtane solution directory", DefaultValueFactory = _ => "." };
var frameworkOption = new Option<string>("--framework") { Description = "Target framework version", DefaultValueFactory = _ => "net10.0" };
var forceOption = new Option<bool>("--force") { Description = "Overwrite existing module if it exists", DefaultValueFactory = _ => false };

createCommand.Add(ownerOption);
createCommand.Add(nameOption);
createCommand.Add(templateOption);
createCommand.Add(descriptionOption);
createCommand.Add(solutionOption);
createCommand.Add(frameworkOption);
createCommand.Add(forceOption);

createCommand.SetAction((parseResult) =>
{
    var owner = parseResult.GetValue(ownerOption)!;
    var name = parseResult.GetValue(nameOption)!;
    var template = parseResult.GetValue(templateOption)!;
    var description = parseResult.GetValue(descriptionOption)!;
    var solution = parseResult.GetValue(solutionOption)!;
    var framework = parseResult.GetValue(frameworkOption)!;
    var force = parseResult.GetValue(forceOption);

    // Validate owner/name are alphanumeric
    if (!owner.All(char.IsLetterOrDigit))
    {
        Console.Error.WriteLine("Error: Owner name must be alphanumeric (A-Z, a-z, 0-9).");
        return 1;
    }

    if (!name.All(char.IsLetterOrDigit))
    {
        Console.Error.WriteLine("Error: Module name must be alphanumeric (A-Z, a-z, 0-9).");
        return 1;
    }

    // Find solution directory
    var startPath = Path.GetFullPath(solution);
    var solutionDir = templateService.FindSolutionDirectory(startPath);
    if (solutionDir is null)
    {
        Console.Error.WriteLine(
            "Error: Cannot find Oqtane solution. " +
            "Run from an Oqtane app directory or use --solution.");
        return 1;
    }

    // Resolve template path
    string templatePath;
    try
    {
        templatePath = templateService.GetTemplatePath(solutionDir, template);
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }

    if (!Directory.Exists(templatePath))
    {
        Console.Error.WriteLine(
            $"Error: Template directory not found at '{templatePath}'. " +
            "Use 'oqtane module list' to see available templates.");
        return 1;
    }

    // Load manifest
    MarkDav.Oqtane.Cli.Models.TemplateManifest manifest;
    try
    {
        manifest = templateService.LoadTemplateManifest(templatePath);
    }
    catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }

    // Print confirmation (dry-run style)
    Console.WriteLine($"Template:  {manifest.Name} (v{manifest.Version})");
    Console.WriteLine($"Owner:     {owner}");
    Console.WriteLine($"Module:    {name}");
    Console.WriteLine($"Framework: {framework}");
    if (!string.IsNullOrWhiteSpace(description))
        Console.WriteLine($"Description: {description}");
    Console.WriteLine();
    Console.WriteLine("Files to be created:");
    foreach (var file in manifest.Files)
    {
        var resolvedPath = file.Path
            .Replace("[Owner]", owner)
            .Replace("[Module]", name);
        Console.WriteLine($"  {resolvedPath}");
    }

    // Actual file generation will be added in Phase 4
    return 0;
});

// ── module list ─────────────────────────────────────────────────────────────
var listCommand = new Command("list", "List available module templates");
moduleCommand.Subcommands.Add(listCommand);

var listSolutionOption = new Option<string>("--solution") { Description = "Path to Oqtane solution directory", DefaultValueFactory = _ => "." };
listCommand.Add(listSolutionOption);

listCommand.SetAction((parseResult) =>
{
    var solution = parseResult.GetValue(listSolutionOption)!;
    var startPath = Path.GetFullPath(solution);
    var solutionDir = templateService.FindSolutionDirectory(startPath);

    if (solutionDir is null)
    {
        Console.Error.WriteLine(
            "Error: Cannot find Oqtane solution. " +
            "Run from an Oqtane app directory or use --solution.");
        return 1;
    }

    var templates = templateService.ListTemplates(solutionDir);

    if (templates.Count == 0)
    {
        Console.WriteLine("No module templates found.");
        return 0;
    }

    Console.WriteLine("Available Module Templates:");
    foreach (var (type, manifest) in templates)
    {
        Console.WriteLine($"  {type,-12} {manifest.Name}");
    }

    return 0;
});

return rootCommand.Parse(args).Invoke();
