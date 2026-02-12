using FluentAssertions;
using MarkDav.Oqtane.Cli.Tests.Helpers;
using Xunit;

namespace MarkDav.Oqtane.Cli.Tests.Integration;

/// <summary>
/// Integration tests for the 'module create' command
/// NOTE: These tests require refactoring Program.cs to support testability.
/// Currently marked as pending until CLI entry point can be invoked programmatically.
/// </summary>
public class ModuleCreateCommandTests
{
    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_WithRequiredParameters_Succeeds()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Acme",
            "--name", "Blog",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("Owner:     Acme");
        helper.Output.Should().Contain("Module:    Blog");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_WithAllParameters_Succeeds()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "MarkDav",
            "--name", "Weather",
            "--template", "internal",
            "--description", "Weather module",
            "--framework", "net9.0",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("Owner:     MarkDav");
        helper.Output.Should().Contain("Module:    Weather");
        helper.Output.Should().Contain("Framework: net9.0");
        helper.Output.Should().Contain("Description: Weather module");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_InvalidOwnerName_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Test-Owner",
            "--name", "Blog",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("Owner name must be alphanumeric");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_InvalidModuleName_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Acme",
            "--name", "Test-Module",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("Module name must be alphanumeric");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_MissingOwner_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--name", "Blog",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("owner");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_MissingName_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Acme",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("name");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_NoSolution_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        // Don't create solution

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Acme",
            "--name", "Blog",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("Cannot find Oqtane solution");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_InvalidTemplate_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Acme",
            "--name", "Blog",
            "--template", "invalid",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("not found");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleCreate_ShowsFilesToBeCreated()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand(
            "module", "create",
            "--owner", "Acme",
            "--name", "Blog",
            "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("Files to be created:");
        helper.Output.Should().Contain("Acme.Blog.Client/Module.cs");
        helper.Output.Should().Contain("Acme.Blog.Server/Controllers/BlogController.cs");
    }
}
