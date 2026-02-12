#!/usr/bin/env bash
# Creates GitHub issues for the Oqtane Module CLI project.
# Usage: ./create-issues.sh
# Requires: gh CLI authenticated (run `gh auth login` first)

set -euo pipefail

REPO="markdav-is/oqtane-module-cli"

echo "Creating issues for $REPO..."
echo ""

# ── Issue 1: Project Setup ──────────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Project Setup: .NET 10 console app with System.CommandLine" \
  --label "phase:0-setup" \
  --body "$(cat <<'BODY'
## Summary
Set up the initial .NET 10 console project with required dependencies and folder structure. This is the foundation everything else builds on.

## Tasks
- [ ] Create `MarkDav.Oqtane.Cli` .NET 10 console application (`dotnet new console`)
- [ ] Add `System.CommandLine` NuGet package
- [ ] Evaluate `Scriban` for template processing (or confirm simple string replacement is sufficient)
- [ ] Create initial folder structure:
  ```
  MarkDav.Oqtane.Cli/
  ├── Commands/
  ├── Services/
  ├── Models/
  └── Program.cs
  ```
- [ ] Configure `.csproj` for global tool packaging:
  ```xml
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>oqtane</ToolCommandName>
  <PackageId>MarkDav.Oqtane.Cli</PackageId>
  <Version>1.0.0</Version>
  ```
- [ ] Verify `dotnet run` produces basic output
- [ ] Verify `dotnet pack` creates a valid NuGet package

## Acceptance Criteria
- Project compiles and runs on .NET 10
- `dotnet tool install --global --add-source ./nupkg MarkDav.Oqtane.Cli` installs successfully
- Running `oqtane --version` prints a version string
- Folder structure matches the spec

## References
- [oqtane-cli-prompt.md — Project Structure](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — NuGet Package Configuration](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 1 created: Project Setup"

# ── Issue 2: CLI Command Parsing ────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Phase 1: CLI command parsing with System.CommandLine" \
  --label "phase:1-cli" \
  --body "$(cat <<'BODY'
## Summary
Implement the command structure using System.CommandLine so the tool accepts and validates user input.

## Depends On
- Project Setup issue

## Tasks
- [ ] Implement root command (`oqtane`)
- [ ] Implement `module` subcommand
- [ ] Implement `module create` command with options:
  - `--owner` (required, string)
  - `--name` (required, string)
  - `--template` (optional, default: `internal`, choices: `internal`/`external`)
  - `--description` (optional, string)
  - `--solution` (optional, default: `.`)
  - `--framework` (optional, default: `net10.0`)
  - `--force` (optional, flag)
- [ ] Add input validation:
  - Owner/name must be alphanumeric (`A-Z, a-z, 0-9`)
  - Template must be `internal` or `external`
- [ ] Wire up `--version` and `--help` output
- [ ] Create `ModuleOptions` model class to hold parsed options
- [ ] Print confirmation of parsed options (placeholder for actual execution)

## Acceptance Criteria
- `oqtane module create --owner Test --name MyModule` parses and prints confirmation
- `oqtane module create --name MyModule` fails with "Error: --owner is required"
- `oqtane module create --owner "bad name!" --name Foo` fails with validation error
- `oqtane --help` shows available commands
- `oqtane --version` prints version

## References
- [oqtane-cli-prompt.md — Phase 1: Basic CLI](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — Command Structure & Option Details](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 2 created: CLI Command Parsing"

# ── Issue 3: Template Discovery ─────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Phase 2: Template discovery and manifest parsing" \
  --label "phase:2-templates" \
  --body "$(cat <<'BODY'
## Summary
Implement `ITemplateService` to locate Oqtane template directories and parse `template.json` manifest files.

## Depends On
- Phase 1: CLI Command Parsing

## Tasks
- [ ] Create `TemplateManifest` model matching `template.json` structure:
  ```json
  {
    "name": "...",
    "version": "...",
    "files": [{ "path": "...", "tokens": [...] }]
  }
  ```
- [ ] Implement `ITemplateService` / `TemplateService`:
  - `FindSolutionDirectory(string startPath)` — walk up directories looking for `.slnx` or `.sln`
  - `GetTemplatePath(string solutionDir, string templateType)` — resolve `Oqtane.Server/wwwroot/Modules/Templates/{Internal|External}/`
  - `LoadTemplateManifest(string templatePath)` — deserialize `template.json`
  - `ListTemplates(string solutionDir)` — enumerate available templates
- [ ] Handle error cases:
  - Solution not found: "Cannot find Oqtane solution. Run from Oqtane app directory or use --solution."
  - Template directory missing
  - Invalid/missing `template.json`
- [ ] Wire into the `module create` command handler

## Acceptance Criteria
- Given an Oqtane solution directory, discovers internal and external template paths
- Parses `template.json` and returns a `TemplateManifest` with file list
- Prints list of files that would be created (dry-run style output)
- Clear error messages when solution or templates are not found

## References
- [oqtane-cli-prompt.md — Phase 2: Template Discovery](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — Template Discovery & Manifest Format](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 3 created: Template Discovery"

# ── Issue 4: Token Replacement ──────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Phase 3: Token replacement service" \
  --label "phase:3-tokens" \
  --body "$(cat <<'BODY'
## Summary
Implement `ITokenReplacementService` to replace Oqtane tokens in file contents and file/directory names.

## Depends On
- Phase 2: Template Discovery (needs `TemplateManifest` model)

## Tasks
- [ ] Implement `ITokenReplacementService` / `TokenReplacementService`:
  - `GenerateTokens(ModuleOptions options)` → `Dictionary<string, string>`
  - `ReplaceTokens(string content, Dictionary<string, string> tokens)` → string
  - `ReplaceTokensInPath(string path, Dictionary<string, string> tokens)` → string
- [ ] Support all standard Oqtane tokens:
  | Token | Replacement |
  |-------|-------------|
  | `[Owner]` | Owner name (e.g., `MarkDav`) |
  | `[Module]` | Module name (e.g., `WeatherArbitrage`) |
  | `[Description]` | Module description |
  | `[Guid]` | New GUID — **unique per occurrence** |
  | `[Year]` | Current year (e.g., `2026`) |
  | `[Date]` | Current date (e.g., `2026-02-12`) |
  | `[Framework]` | Target framework (e.g., `net10.0`) |
- [ ] Ensure `[Guid]` generates a **distinct** GUID each time it appears (not one GUID reused everywhere)
- [ ] Handle token replacement in both file contents and file/directory paths

## Acceptance Criteria
- Unit tests pass for all token types
- Multiple `[Guid]` tokens in the same file produce different GUIDs
- Tokens in file paths are replaced (e.g., `[Owner].[Module].Client/` → `MarkDav.WeatherArbitrage.Client/`)
- Content with no tokens passes through unchanged

## References
- [oqtane-cli-prompt.md — Token Replacement](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — Token Replacement](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 4 created: Token Replacement"

# ── Issue 5: File Generation ────────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Phase 4: File generation from templates" \
  --label "phase:4-files" \
  --body "$(cat <<'BODY'
## Summary
Implement `IFileService` to create the module directory structure and copy/process template files with token replacement.

## Depends On
- Phase 3: Token Replacement Service
- Phase 2: Template Discovery

## Tasks
- [ ] Implement `IFileService` / `FileService`:
  - `ModuleExists(string solutionDir, string owner, string moduleName)` → bool
  - `CreateModuleStructure(string targetDir, TemplateManifest manifest, Dictionary<string, string> tokens)` → void
  - `DeleteModule(string targetDir, string owner, string moduleName)` — for `--force` overwrite
- [ ] For each file in the template manifest:
  1. Read source template file
  2. Apply token replacement to file contents
  3. Apply token replacement to target file path
  4. Create target directories as needed
  5. Write processed file to target location
- [ ] Handle `--force` flag: delete existing module directories before creating
- [ ] Skip binary files (don't apply token replacement to non-text files)

## Acceptance Criteria
- Running `module create` with a real Oqtane template produces the correct file tree:
  ```
  MarkDav.WeatherArbitrage.Client/
  MarkDav.WeatherArbitrage.Server/
  MarkDav.WeatherArbitrage.Shared/
  ```
- All `[Owner]`, `[Module]`, etc. tokens are replaced in file contents
- File and directory names have tokens replaced
- `--force` removes existing module before recreating
- Error when module exists and `--force` is not set

## References
- [oqtane-cli-prompt.md — Phase 4: File Creation](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — File Operations](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 5 created: File Generation"

# ── Issue 6: Solution File Integration ──────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Phase 5: Solution file integration (.sln and .slnx)" \
  --label "phase:5-solution" \
  --body "$(cat <<'BODY'
## Summary
Implement `ISolutionService` to detect solution file format and add newly created module projects to the solution.

## Depends On
- Phase 4: File Generation (needs projects to exist on disk)

## Tasks
- [ ] Implement `ISolutionService` / `SolutionService`:
  - `FindSolutionFile(string directory)` → string (path to .slnx or .sln)
  - `DetectFormat(string solutionPath)` → `SolutionFormat` enum (Slnx | Sln)
  - `AddProjects(string solutionPath, List<string> projectPaths)` → void
- [ ] **.slnx support** (JSON-based, .NET 10+):
  - Parse existing .slnx JSON
  - Append new project entries to the `projects` array
  - Write back with proper formatting
- [ ] **.sln support** (classic text format):
  - Parse existing .sln to find insertion point
  - Add `Project("...")` entries with new GUIDs
  - Add to solution configuration platforms
- [ ] Verify solution file is valid after modification:
  - `dotnet sln list` should show new projects
  - `dotnet build` should not fail due to solution file issues

## Acceptance Criteria
- New projects are added to .slnx files correctly
- New projects are added to .sln files correctly
- `dotnet sln list` shows the new projects after modification
- Solution file format is auto-detected
- Existing projects in the solution are not disturbed

## References
- [oqtane-cli-prompt.md — Phase 5: Solution Integration](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — Solution File Management](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 6 created: Solution File Integration"

# ── Issue 7: Error Handling & Polish ────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Phase 6: Error handling, validation, and success messages" \
  --label "phase:6-polish" \
  --body "$(cat <<'BODY'
## Summary
Add robust error handling, user-friendly messages, and overall UX polish to the CLI.

## Depends On
- All previous phases (1–5)

## Tasks
- [ ] Implement all specified error messages:
  - No solution file found
  - Module already exists (suggest `--force`)
  - Invalid owner/module name (must be alphanumeric)
  - Template not found (suggest `oqtane module list`)
- [ ] Implement success message with summary:
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
- [ ] Add exit codes (0 = success, 1 = validation error, 2 = runtime error)
- [ ] Ensure no unhandled exceptions leak to the user
- [ ] Add `--verbose` or `--debug` flag for troubleshooting (nice-to-have)

## Acceptance Criteria
- Every error scenario from the spec produces a clear, actionable message
- Successful creation prints the formatted summary with next steps
- Non-zero exit codes on failure
- No stack traces shown to users in normal operation

## References
- [oqtane-cli-prompt.md — Error Cases to Handle](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — Error Handling & Success Messages](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 7 created: Error Handling & Polish"

# ── Issue 8: Module List Command ────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Implement 'oqtane module list' command" \
  --label "phase:6-polish" \
  --body "$(cat <<'BODY'
## Summary
Implement the `oqtane module list` command to show available module templates.

## Depends On
- Phase 2: Template Discovery (reuses `ITemplateService.ListTemplates`)

## Tasks
- [ ] Add `module list` subcommand to System.CommandLine tree
- [ ] Query `ITemplateService` for available templates
- [ ] Display formatted output:
  ```
  Available Module Templates:
    internal   Oqtane 10.0 Internal Module Template
    external   Oqtane 10.0 External Module Template
  ```
- [ ] Handle case where no templates are found
- [ ] Optionally accept `--solution` path to look in a specific directory

## Acceptance Criteria
- `oqtane module list` prints available templates with name and description
- Works from within an Oqtane solution directory
- Clear message if no templates found

## References
- [oqtane-module-cli-specification.md — List Available Templates](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 8 created: Module List Command"

# ── Issue 9: Testing Suite ──────────────────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "Add unit and integration test suite" \
  --label "testing" \
  --body "$(cat <<'BODY'
## Summary
Create a test project with unit tests for core services and integration tests for the end-to-end workflow.

## Depends On
- Phases 1–5 (test as each phase lands)

## Tasks
- [ ] Create `MarkDav.Oqtane.Cli.Tests` xUnit test project
- [ ] **Unit tests:**
  - [ ] Token replacement — all token types, multiple GUIDs, path replacement
  - [ ] Template manifest parsing — valid JSON, missing fields, malformed
  - [ ] Solution file manipulation — .slnx add project, .sln add project
  - [ ] Input validation — valid/invalid owner names, module names
- [ ] **Integration tests:**
  - [ ] Full `module create` workflow with a sample template fixture
  - [ ] Verify generated file contents have correct token replacements
  - [ ] Verify solution file is updated
  - [ ] Error scenarios (module exists, template not found)
- [ ] Add test fixtures:
  - Sample `template.json`
  - Sample template files with tokens
  - Sample `.slnx` and `.sln` files

## Acceptance Criteria
- `dotnet test` passes on all platforms
- Code coverage for token replacement ≥ 90%
- Integration test creates and verifies a complete module from fixtures
- Tests run in CI (once CI is set up)

## Testing Guidance from Spec
> - Unit tests for token replacement logic
> - Integration test with real Oqtane template
> - Manual test by creating a test module in actual Oqtane app and verifying it compiles

## References
- [oqtane-cli-prompt.md — Testing Approach](specs/oqtane-cli-prompt.md)
- [oqtane-module-cli-specification.md — Testing Strategy](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 9 created: Testing Suite"

# ── Issue 10: NuGet Packaging & CI/CD ───────────────────────────────────────
gh issue create --repo "$REPO" \
  --title "NuGet packaging, GitHub Actions CI/CD, and release workflow" \
  --label "infrastructure" \
  --body "$(cat <<'BODY'
## Summary
Set up NuGet packaging, CI/CD pipeline, and release workflow so the tool can be distributed.

## Depends On
- All implementation phases
- Testing Suite

## Tasks
- [ ] **NuGet packaging:**
  - Finalize `.csproj` metadata (authors, description, tags, license, repo URL)
  - `dotnet pack -c Release` produces valid `.nupkg`
  - Test local install: `dotnet tool install --global --add-source ./nupkg MarkDav.Oqtane.Cli`
- [ ] **GitHub Actions CI:**
  - Build on push/PR to main
  - Run `dotnet test` on Windows, Linux, macOS
  - Pack and upload artifact
- [ ] **Release workflow:**
  - Tag-based release trigger (e.g., `v1.0.0`)
  - Publish `.nupkg` to NuGet.org
  - Create GitHub Release with notes
- [ ] **Versioning:**
  - Semantic versioning (1.0.x for Oqtane 10.0.x)
  - Version set in `.csproj` or via CI

## Acceptance Criteria
- `dotnet pack` produces installable global tool
- CI builds and tests pass on all three platforms
- Tagged releases auto-publish to NuGet.org
- README has install instructions

## References
- [oqtane-module-cli-specification.md — Distribution](specs/oqtane-module-cli-specification.md)
- [oqtane-module-cli-specification.md — Compatibility Matrix](specs/oqtane-module-cli-specification.md)
BODY
)"
echo "✓ Issue 10 created: NuGet Packaging & CI/CD"

echo ""
echo "════════════════════════════════════════════"
echo "  All 10 issues created successfully!"
echo "  View them: gh issue list --repo $REPO"
echo "════════════════════════════════════════════"
