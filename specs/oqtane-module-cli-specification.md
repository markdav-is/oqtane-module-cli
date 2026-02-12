# Oqtane Module CLI Tool Specification

## Overview
A .NET CLI tool to automate the creation of Oqtane modules, eliminating the need for manual web UI interaction during module scaffolding.

## Problem Statement
Currently, creating an Oqtane module requires:
1. Running the Oqtane application
2. Logging into the admin dashboard
3. Navigating to Module Management
4. Filling out a web form to create the module scaffold

This breaks the flow of CLI-based development and cannot be automated in scripts or CI/CD pipelines.

## Solution
A standalone .NET global tool that:
- Reads Oqtane's module template definitions
- Performs token replacement and file generation
- Creates the proper project structure
- Updates solution files
- Works entirely from the command line

## Tool Name
`MarkDav.Oqtane.Cli` (or `Oqtane.Cli` if contributed to the Oqtane project)

## Installation

```bash
# Install as global tool
dotnet tool install -g MarkDav.Oqtane.Cli

# Update to latest version
dotnet tool update -g MarkDav.Oqtane.Cli

# Uninstall
dotnet tool uninstall -g MarkDav.Oqtane.Cli
```

## Command Structure

```bash
oqtane module create [options]
oqtane module list
oqtane theme create [options]
oqtane --version
oqtane --help
```

## Core Command: Module Create

### Basic Usage
```bash
oqtane module create \
  --owner MarkDav \
  --name WeatherArbitrage \
  --template internal \
  --description "Weather market arbitrage tracker"
```

### Full Options
```bash
oqtane module create \
  --owner <OwnerName> \
  --name <ModuleName> \
  --template <internal|external> \
  --description "<module description>" \
  --solution <path-to-solution-directory> \
  --framework <net10.0|net9.0|net8.0> \
  --force
```

### Option Details

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `--owner` | Yes | - | Owner/company name for namespace (e.g., MarkDav) |
| `--name` | Yes | - | Module name (e.g., WeatherArbitrage) |
| `--template` | No | `internal` | Template type: `internal` or `external` |
| `--description` | No | "" | Brief module description |
| `--solution` | No | `.` | Path to Oqtane solution directory |
| `--framework` | No | `net10.0` | Target framework version |
| `--force` | No | false | Overwrite existing module if it exists |

## Template Types

### Internal Template
- Creates module inside the existing Oqtane application solution
- Used for building applications with embedded modules
- Projects are part of the main solution
- Best for custom applications (not marketplace distribution)

**Generated Structure:**
```
<Solution>/
├── <Owner>.<Name>.Client/
│   └── Modules/<Owner>.<Name>/
│       ├── Index.razor
│       ├── Edit.razor
│       └── Settings.razor
├── <Owner>.<Name>.Server/
│   ├── Controllers/<Name>Controller.cs
│   ├── Repository/
│   ├── Manager/
│   └── Migrations/
└── <Owner>.<Name>.Shared/
    └── Models/<Name>.cs
```

### External Template
- Creates standalone module solution separate from Oqtane framework
- Used for building distributable modules
- Includes its own solution file
- Best for marketplace/community modules

**Generated Structure:**
```
<Owner>.<Name>/
├── <Owner>.<Name>.sln
├── <Owner>.<Name>.Client/
├── <Owner>.<Name>.Server/
├── <Owner>.<Name>.Shared/
└── <Owner>.<Name>.Package/
```

## Implementation Requirements

### 1. Template Discovery
The tool must locate and parse Oqtane's template definitions:

**Internal Templates:**
- Location: `Oqtane.Server/wwwroot/Modules/Templates/Internal/`
- Manifest: `template.json`

**External Templates:**
- Location: `Oqtane.Server/wwwroot/Modules/Templates/External/`
- Manifest: `template.json`

### 2. Template Manifest Format
Parse Oqtane's `template.json` structure:

```json
{
  "name": "Internal Module Template",
  "version": "1.0.0",
  "owner": "[Owner]",
  "module": "[Module]",
  "description": "[Description]",
  "files": [
    {
      "path": "Client/Modules/[Owner].[Module]/Index.razor",
      "tokens": ["[Owner]", "[Module]", "[Description]"]
    }
  ]
}
```

### 3. Token Replacement
Support standard Oqtane tokens:
- `[Owner]` → Owner name
- `[Module]` → Module name
- `[Description]` → Module description
- `[Guid]` → New GUID (generate unique for each occurrence)
- `[Year]` → Current year
- `[Date]` → Current date
- `[Framework]` → Target framework (e.g., net10.0)

### 4. File Operations
The tool must:
1. **Create directory structure** following template layout
2. **Copy template files** with token replacement
3. **Generate new GUIDs** for project files
4. **Update solution file** (.sln or .slnx format)
5. **Set file permissions** if needed

### 5. Solution File Management
Handle both .NET solution formats:
- **Classic .sln format** (XML-based)
- **New .slnx format** (JSON-based, .NET 10+)

**Example: Adding project to .slnx**
```json
{
  "solution": {
    "projects": [
      {
        "path": "MarkDav.WeatherArbitrage.Client/MarkDav.WeatherArbitrage.Client.csproj"
      },
      {
        "path": "MarkDav.WeatherArbitrage.Server/MarkDav.WeatherArbitrage.Server.csproj"
      }
    ]
  }
}
```

## Additional Commands

### List Available Templates
```bash
oqtane module list

# Output:
# Available Module Templates:
# - internal (Oqtane 10.0 Internal Module Template)
# - external (Oqtane 10.0 External Module Template)
```

### Create Theme
```bash
oqtane theme create \
  --owner MarkDav \
  --name CustomTheme \
  --template internal
```

## Error Handling

### Validation Errors
- **Missing required option:** "Error: --owner is required"
- **Invalid owner name:** "Error: Owner name must be alphanumeric (A-Z, a-z, 0-9)"
- **Invalid module name:** "Error: Module name must be alphanumeric"
- **Module already exists:** "Error: Module MarkDav.WeatherArbitrage already exists. Use --force to overwrite"
- **Solution not found:** "Error: Cannot find Oqtane solution in current directory"
- **Template not found:** "Error: Template 'invalid' not found. Use 'oqtane module list' to see available templates"

### Success Messages
```
✓ Module created successfully!
  
  Owner: MarkDav
  Name: WeatherArbitrage
  Template: Internal
  Location: ./MarkDav.WeatherArbitrage.Client
           ./MarkDav.WeatherArbitrage.Server
           ./MarkDav.WeatherArbitrage.Shared
  
  Next steps:
  1. Build the solution: dotnet build
  2. Run the application: cd AppHost && dotnet run
  3. Navigate to your module in the Oqtane admin dashboard
```

## Technical Implementation

### Technology Stack
- **.NET 10** (or latest LTS)
- **System.CommandLine** for CLI parsing
- **Microsoft.Extensions.FileProviders** for template file access
- **Scriban** or **RazorLight** for template processing
- **Newtonsoft.Json** or **System.Text.Json** for manifest parsing

### Project Structure
```
MarkDav.Oqtane.Cli/
├── Commands/
│   ├── ModuleCommand.cs
│   ├── ThemeCommand.cs
│   └── VersionCommand.cs
├── Services/
│   ├── TemplateService.cs
│   ├── FileService.cs
│   ├── SolutionService.cs
│   └── TokenReplacementService.cs
├── Models/
│   ├── TemplateManifest.cs
│   ├── ModuleOptions.cs
│   └── ThemeOptions.cs
├── Templates/
│   └── (embedded template resources)
└── Program.cs
```

### Key Classes

**ITemplateService**
```csharp
public interface ITemplateService
{
    Task<TemplateManifest> GetTemplateAsync(string templateName, string templateType);
    Task<List<string>> ListTemplatesAsync(string templateType);
    Task<bool> ValidateTemplateAsync(string templateName);
}
```

**IFileService**
```csharp
public interface IFileService
{
    Task CreateDirectoryStructureAsync(string basePath, TemplateManifest manifest);
    Task CopyTemplateFilesAsync(string templatePath, string targetPath, Dictionary<string, string> tokens);
    Task<bool> ModuleExistsAsync(string solutionPath, string moduleName);
}
```

**ISolutionService**
```csharp
public interface ISolutionService
{
    Task<string> FindSolutionFileAsync(string directory);
    Task AddProjectsToSolutionAsync(string solutionPath, List<string> projectPaths);
    SolutionFormat DetectSolutionFormat(string solutionPath);
}
```

**ITokenReplacementService**
```csharp
public interface ITokenReplacementService
{
    Dictionary<string, string> GenerateTokens(ModuleOptions options);
    string ReplaceTokens(string content, Dictionary<string, string> tokens);
    string GenerateGuid();
}
```

## NuGet Package Configuration

**MarkDav.Oqtane.Cli.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>oqtane</ToolCommandName>
    <PackageId>MarkDav.Oqtane.Cli</PackageId>
    <Version>1.0.0</Version>
    <Authors>MarkDav</Authors>
    <Description>CLI tool for creating Oqtane modules and themes</Description>
    <PackageTags>oqtane;blazor;module;cli;scaffolding</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/markdav/oqtane-cli</RepositoryUrl>
  </PropertyGroup>
</Project>
```

## Testing Strategy

### Unit Tests
- Token replacement logic
- Template manifest parsing
- Solution file manipulation
- File path validation

### Integration Tests
- Full module creation workflow
- Template discovery
- Solution file updates
- Error scenarios

### Manual Testing Checklist
- [ ] Create internal module in new Oqtane app
- [ ] Create external module standalone
- [ ] Verify generated code compiles
- [ ] Verify solution file is valid
- [ ] Test with both .sln and .slnx formats
- [ ] Test --force overwrite flag
- [ ] Test invalid inputs
- [ ] Test on Windows, Linux, macOS

## Distribution

### NuGet.org Publication
```bash
# Build and pack
dotnet pack -c Release

# Publish to NuGet
dotnet nuget push bin/Release/MarkDav.Oqtane.Cli.1.0.0.nupkg \
  --api-key <key> \
  --source https://api.nuget.org/v3/index.json
```

### GitHub Repository
- Repository: `markdav/oqtane-cli`
- License: MIT
- README with usage examples
- CI/CD via GitHub Actions
- Automated testing on PRs
- Semantic versioning

## Future Enhancements

### Phase 2 Features
- **Interactive mode:** Prompt for options if not provided
- **Module templates:** Allow custom user templates
- **Validation:** Pre-flight checks for dependencies
- **Update command:** Update existing modules to new Oqtane versions
- **Scaffold entities:** Add models, controllers, repositories to existing modules

### Example: Interactive Mode
```bash
oqtane module create --interactive

? Owner name: MarkDav
? Module name: WeatherArbitrage
? Description: Weather market arbitrage tracker
? Template (internal/external): internal
? Target framework (net10.0): [Enter]
✓ Creating module...
```

### Example: Scaffold Entity
```bash
oqtane entity add \
  --module WeatherArbitrage \
  --name MarketOdds \
  --properties "Platform:string,MarketId:string,YesProbability:decimal"
```

## Compatibility Matrix

| Tool Version | Oqtane Version | .NET Version |
|-------------|----------------|--------------|
| 1.0.x       | 10.0.x         | .NET 10      |
| 0.9.x       | 9.0.x          | .NET 9       |
| 0.8.x       | 8.0.x          | .NET 8       |

## References
- Oqtane Framework: https://github.com/oqtane/oqtane.framework
- Oqtane Docs: https://docs.oqtane.org
- System.CommandLine: https://github.com/dotnet/command-line-api
- .NET Global Tools: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools

## Success Criteria
- ✅ Tool installs as global .NET tool
- ✅ Creates valid Oqtane modules via CLI
- ✅ Generated code compiles without errors
- ✅ Works on Windows, Linux, macOS
- ✅ Supports both internal and external templates
- ✅ Updates solution files correctly
- ✅ Provides clear error messages
- ✅ Documentation covers all use cases

## Project Timeline (Estimated)

- **Week 1:** Project setup, template discovery, manifest parsing
- **Week 2:** Token replacement, file generation
- **Week 3:** Solution file management, testing
- **Week 4:** Documentation, NuGet packaging, release

## License
MIT License - Compatible with Oqtane's MIT license

---

**Document Version:** 1.0  
**Last Updated:** February 12, 2026  
**Author:** MarkDav  
**Status:** Specification - Ready for Implementation
