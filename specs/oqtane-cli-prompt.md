# Build Oqtane Module CLI Tool

Create a .NET global tool that automates Oqtane module scaffolding from the command line, eliminating the need for web UI interaction.

## What to Build

A CLI tool that developers can install and use like this:

```bash
# Install globally
dotnet tool install -g MarkDav.Oqtane.Cli

# Create a module
oqtane module create \
  --owner MarkDav \
  --name WeatherArbitrage \
  --template internal \
  --description "Weather market arbitrage tracker"

# Result: Module projects created and added to solution
```

## Core Requirements

### 1. Command Structure
```bash
oqtane module create [options]
oqtane module list
oqtane --version
oqtane --help
```

**Required Options:**
- `--owner` - Company/owner name (e.g., MarkDav)
- `--name` - Module name (e.g., WeatherArbitrage)

**Optional Options:**
- `--template` - Template type: `internal` (default) or `external`
- `--description` - Module description
- `--solution` - Path to solution directory (default: current directory)
- `--force` - Overwrite if exists

### 2. What It Does

**Input:** Command with owner/name/template
**Process:**
1. Find Oqtane solution directory (look for .slnx or .sln file)
2. Locate template files in `wwwroot/Modules/Templates/Internal/` (or External)
3. Parse `template.json` manifest to understand file structure
4. Copy template files to target locations
5. Replace tokens: `[Owner]`, `[Module]`, `[Description]`, `[Guid]`
6. Generate new GUIDs for project files
7. Add new projects to solution file (.slnx format)

**Output:** Working module projects integrated into solution

### 3. Template Structure to Support

Oqtane's internal templates live in:
```
Oqtane.Server/wwwroot/Modules/Templates/Internal/
├── template.json          # Manifest
├── [Owner].[Module].Client/
├── [Owner].[Module].Server/
└── [Owner].[Module].Shared/
```

The `template.json` defines which files to copy and what tokens to replace.

### 4. Token Replacement

Support these standard tokens:
- `[Owner]` → MarkDav
- `[Module]` → WeatherArbitrage  
- `[Description]` → "Weather market arbitrage tracker"
- `[Guid]` → Generate new GUID (unique per occurrence)
- `[Year]` → 2026
- `[Date]` → 2026-02-12
- `[Framework]` → net10.0

## Technical Stack

- **.NET 10** as target framework
- **System.CommandLine** for CLI parsing
- **Scriban** for template processing (or simple string replacement)
- **System.Text.Json** for parsing template.json
- Package as .NET Global Tool

## Project Structure

```
MarkDav.Oqtane.Cli/
├── Commands/
│   └── ModuleCreateCommand.cs
├── Services/
│   ├── TemplateService.cs
│   ├── TokenReplacementService.cs
│   ├── FileService.cs
│   └── SolutionService.cs
├── Models/
│   ├── TemplateManifest.cs
│   └── ModuleOptions.cs
└── Program.cs
```

## Implementation Strategy

### Phase 1: Basic CLI (Start Here)
1. Set up .NET console app with System.CommandLine
2. Parse `oqtane module create` with required options
3. Validate inputs (owner/name are alphanumeric)
4. Print success message with provided options

**Test:** `oqtane module create --owner Test --name MyModule` → prints confirmation

### Phase 2: Template Discovery
1. Find Oqtane solution directory (search up for .slnx file)
2. Locate template directory: `Oqtane.Server/wwwroot/Modules/Templates/Internal/`
3. Read and parse `template.json`
4. List all files that need to be copied

**Test:** Tool finds template.json and lists files to create

### Phase 3: Token Replacement
1. Implement token replacement service
2. Read template file content
3. Replace all tokens with actual values
4. Generate unique GUIDs for each `[Guid]` occurrence

**Test:** Template file with tokens → replaced content

### Phase 4: File Creation
1. Create target directory structure
2. Copy and process each template file
3. Apply token replacement
4. Write to correct locations

**Test:** Files created in `<Owner>.<Module>.Client/`, etc.

### Phase 5: Solution Integration
1. Parse .slnx file (JSON format)
2. Add new project entries
3. Write updated solution file

**Test:** Solution file updated, `dotnet build` works

### Phase 6: Polish
1. Add error handling (template not found, module exists, etc.)
2. Add validation (valid owner/module names)
3. Add success messages with next steps
4. Add `--force` flag to overwrite
5. Add `module list` command

## Key Interfaces

### ITemplateService
```csharp
public interface ITemplateService
{
    Task<TemplateManifest> LoadTemplateAsync(string templateType);
    Task<string> GetTemplatePathAsync(string solutionDir, string templateType);
}
```

### ITokenReplacementService
```csharp
public interface ITokenReplacementService
{
    Dictionary<string, string> GenerateTokens(string owner, string module, string description);
    string ReplaceTokens(string content, Dictionary<string, string> tokens);
}
```

### IFileService
```csharp
public interface IFileService
{
    Task CreateModuleStructureAsync(string targetDir, TemplateManifest template, Dictionary<string, string> tokens);
    bool ModuleExists(string solutionDir, string moduleName);
}
```

### ISolutionService
```csharp
public interface ISolutionService
{
    Task<string> FindSolutionFileAsync(string directory);
    Task AddProjectsAsync(string solutionPath, List<string> projectPaths);
}
```

## Package Configuration

Make it a global tool in the .csproj:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>oqtane</ToolCommandName>
  <PackageId>MarkDav.Oqtane.Cli</PackageId>
  <Version>1.0.0</Version>
</PropertyGroup>
```

## Success Criteria

When complete, this should work end-to-end:

```bash
# In an Oqtane application directory
dotnet tool install -g MarkDav.Oqtane.Cli

oqtane module create --owner MarkDav --name TestModule

# Should create:
# - MarkDav.TestModule.Client/ project
# - MarkDav.TestModule.Server/ project  
# - MarkDav.TestModule.Shared/ project
# - Updated solution file with new projects

dotnet build  # Should compile successfully
```

## Error Cases to Handle

- No solution file found: "Error: Cannot find Oqtane solution. Run from Oqtane app directory."
- Module already exists: "Error: Module already exists. Use --force to overwrite."
- Invalid owner name: "Error: Owner must be alphanumeric (A-Z, a-z, 0-9)"
- Template not found: "Error: Template 'xyz' not found."

## Testing Approach

1. **Unit tests** for token replacement logic
2. **Integration test** with real Oqtane template
3. **Manual test** by creating a test module in actual Oqtane app and verifying it compiles

## Nice-to-Have (Don't Block on These)

- Interactive mode if options missing
- Colorized console output
- Progress indicators
- Support for external templates
- Theme creation command

## Getting Started

1. Create new .NET 10 console app
2. Add System.CommandLine package
3. Implement basic command that prints options
4. Iterate through phases above

## Reference Documents

See `oqtane-module-cli-specification.md` for complete detailed requirements, template format examples, and future enhancements.

---

**Focus:** Build a working tool that creates valid Oqtane modules via CLI. Prioritize the core workflow over edge cases and polish.
