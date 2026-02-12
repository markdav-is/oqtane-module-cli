# Oqtane Module CLI Tests

This project contains unit and integration tests for the Oqtane Module CLI tool.

## Test Structure

```
tests/MarkDav.Oqtane.Cli.Tests/
??? Services/                          # Unit tests for service classes
?   ??? TokenReplacementServiceTests.cs
?   ??? TemplateServiceTests.cs
??? Integration/                       # End-to-end CLI tests
?   ??? ModuleListCommandTests.cs
?   ??? ModuleCreateCommandTests.cs
??? Helpers/                           # Test utilities
?   ??? CliTestHelper.cs
??? MarkDav.Oqtane.Cli.Tests.csproj
```

## Test Frameworks

- **xUnit** - Primary test framework (recommended for .NET)
- **FluentAssertions** - Readable assertion library
- **coverlet** - Code coverage collector

## Running Tests

### Visual Studio
- Open Test Explorer: `Test > Test Explorer`
- Click "Run All Tests"

### Command Line
```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~TokenReplacementServiceTests"
```

## Test Categories

### ? Unit Tests (Currently Working)
Tests for `TokenReplacementService` and `TemplateService` classes that verify:
- Token generation and replacement
- Template discovery and manifest parsing
- File path resolution
- Error handling

**Status**: All unit tests are functional and passing.

### ?? Integration Tests (Pending CLI Refactoring)
End-to-end tests that run actual CLI commands and validate output.

**Status**: Currently skipped. These require refactoring `Program.cs` to support programmatic invocation.

#### To Enable Integration Tests:

1. **Extract command logic from `Program.cs`** into a class like `CommandHandler`:
   ```csharp
   public class CommandHandler
   {
       public int ExecuteCommand(string[] args, TextWriter output, TextWriter error)
       {
           // Move CLI logic here
       }
   }
   ```

2. **Update `Program.cs`** to use the handler:
   ```csharp
   public class Program
   {
       public static int Main(string[] args)
       {
           var handler = new CommandHandler();
           return handler.ExecuteCommand(args, Console.Out, Console.Error);
       }
   }
   ```

3. **Remove `[Skip]` attributes** from integration tests in:
   - `Integration/ModuleListCommandTests.cs`
   - `Integration/ModuleCreateCommandTests.cs`

4. **Update `CliTestHelper.RunCommand()`** to invoke the handler directly.

## Writing New Tests

### Unit Test Example
```csharp
[Fact]
public void MyService_DoesSomething_ReturnsExpected()
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = service.DoSomething();
    
    // Assert
    result.Should().Be(expectedValue);
}
```

### Integration Test Example (when enabled)
```csharp
[Fact]
public void Command_WithParameters_Succeeds()
{
    // Arrange
    using var helper = new CliTestHelper();
    helper.CreateMockOqtaneSolution();
    
    // Act
    var exitCode = helper.RunCommand("module", "list");
    
    // Assert
    exitCode.Should().Be(0);
    helper.Output.Should().Contain("expected output");
}
```

## Test Helpers

### CliTestHelper
Provides utilities for integration testing:
- `CreateMockOqtaneSolution()` - Sets up test directory with Oqtane structure
- `CreateTemplateDirectory()` - Creates template manifests for testing
- `RunCommand()` - Executes CLI commands (requires refactoring)
- Auto-cleanup of test directories

## Code Coverage

To generate a code coverage report:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Continuous Integration

These tests are designed to run in CI/CD pipelines. Add to your workflow:
```yaml
- name: Run tests
  run: dotnet test --no-build --verbosity normal
```

## Related Documentation

- BDD Feature files: `../../src/Specs/Features/`
- Service implementations: `../../src/MarkDav.Oqtane.Cli/Services/`
- CLI entry point: `../../src/MarkDav.Oqtane.Cli/Program.cs`
