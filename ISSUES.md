# Oqtane Module CLI — Issue Breakdown

Work breakdown for the Oqtane Module CLI tool, organized by implementation phase.
Run `.\create-issues.ps1` in PowerShell to create these as GitHub issues (requires `gh` CLI authenticated).

---

## Dependency Graph

```
1. Project Setup
   └─► 2. CLI Command Parsing (Phase 1)
        └─► 3. Template Discovery (Phase 2)
             ├─► 4. Token Replacement (Phase 3)
             │    └─► 5. File Generation (Phase 4)
             │         └─► 6. Solution Integration (Phase 5)
             │              └─► 7. Error Handling & Polish (Phase 6)
             └─► 8. Module List Command
9. Testing (parallel with phases 1–5)
10. NuGet Packaging & CI/CD (after all phases)
```

---

## Issue 1: Project Setup — .NET 10 console app with System.CommandLine

**Label:** `phase:0-setup`

Create `MarkDav.Oqtane.Cli` .NET 10 console app, add System.CommandLine, configure `.csproj`
for global tool packaging, verify `dotnet pack` produces a valid NuGet package.

**Done when:** `oqtane --version` prints a version string after global tool install.

---

## Issue 2: Phase 1 — CLI command parsing with System.CommandLine

**Label:** `phase:1-cli` | **Depends on:** Issue 1

Implement root command, `module create` subcommand with all options (`--owner`, `--name`,
`--template`, `--description`, `--solution`, `--framework`, `--force`). Add input validation
(alphanumeric owner/name). Create `ModuleOptions` model.

**Done when:** `oqtane module create --owner Test --name MyModule` parses and prints confirmation;
missing/invalid options produce clear errors.

---

## Issue 3: Phase 2 — Template discovery and manifest parsing

**Label:** `phase:2-templates` | **Depends on:** Issue 2

Implement `ITemplateService` to find Oqtane solution directory (walk up for `.slnx`/`.sln`),
locate `Oqtane.Server/wwwroot/Modules/Templates/{Internal|External}/`, parse `template.json`.

**Done when:** Given an Oqtane solution directory, the tool finds templates and prints the list
of files it would create.

---

## Issue 4: Phase 3 — Token replacement service

**Label:** `phase:3-tokens` | **Depends on:** Issue 3

Implement `ITokenReplacementService` supporting all Oqtane tokens: `[Owner]`, `[Module]`,
`[Description]`, `[Guid]` (unique per occurrence), `[Year]`, `[Date]`, `[Framework]`.
Replace tokens in both file contents and file/directory paths.

**Done when:** Unit tests pass for all token types; multiple `[Guid]` tokens produce distinct values.

---

## Issue 5: Phase 4 — File generation from templates

**Label:** `phase:4-files` | **Depends on:** Issues 3, 4

Implement `IFileService` to create module directory structure, copy template files with token
replacement applied, handle `--force` overwrite, skip binary files.

**Done when:** `module create` with a real Oqtane template produces the correct
`Owner.Module.Client/Server/Shared/` file tree with all tokens replaced.

---

## Issue 6: Phase 5 — Solution file integration (.sln and .slnx)

**Label:** `phase:5-solution` | **Depends on:** Issue 5

Implement `ISolutionService` to auto-detect `.slnx` (JSON) vs `.sln` (text) format,
add new project entries, and write back. Verify with `dotnet sln list`.

**Done when:** New projects appear in solution file; `dotnet build` does not fail due to
solution file issues; existing projects are undisturbed.

---

## Issue 7: Phase 6 — Error handling, validation, and success messages

**Label:** `phase:6-polish` | **Depends on:** Issues 1–6

Add all specified error messages (no solution found, module exists, invalid names, template
not found). Add formatted success message with next steps. Add exit codes. No unhandled
exceptions leak to the user.

**Done when:** Every error scenario from the spec produces a clear, actionable message;
successful creation prints the formatted summary.

---

## Issue 8: Implement `oqtane module list` command

**Label:** `phase:6-polish` | **Depends on:** Issue 3

Add `module list` subcommand that queries `ITemplateService` and displays available templates
with name and description.

**Done when:** `oqtane module list` prints available templates from an Oqtane solution directory.

---

## Issue 9: Add unit and integration test suite

**Label:** `testing` | **Parallel with:** Issues 2–6

Create `MarkDav.Oqtane.Cli.Tests` xUnit project. Unit tests for token replacement, manifest
parsing, solution manipulation, input validation. Integration tests for full module creation
workflow with fixture templates.

**Done when:** `dotnet test` passes; token replacement coverage >= 90%.

---

## Issue 10: NuGet packaging, GitHub Actions CI/CD, and release workflow

**Label:** `infrastructure` | **Depends on:** All issues

Finalize NuGet metadata, set up GitHub Actions (build + test on Win/Linux/macOS), tag-based
release to NuGet.org, semantic versioning aligned with Oqtane compatibility matrix.

**Done when:** Tagged releases auto-publish to NuGet.org; CI passes on all platforms.
