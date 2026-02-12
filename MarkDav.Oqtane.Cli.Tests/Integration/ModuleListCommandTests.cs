using FluentAssertions;
using MarkDav.Oqtane.Cli.Tests.Helpers;
using Xunit;

namespace MarkDav.Oqtane.Cli.Tests.Integration;

/// <summary>
/// Integration tests for the 'module list' command
/// NOTE: These tests require refactoring Program.cs to support testability.
/// Currently marked as pending until CLI entry point can be invoked programmatically.
/// </summary>
public class ModuleListCommandTests
{
    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleList_WithValidSolution_ListsTemplates()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();

        // Act
        var exitCode = helper.RunCommand("module", "list", "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("Available Module Templates:");
        helper.Output.Should().Contain("internal");
        helper.Output.Should().Contain("external");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleList_NoSolution_ReturnsError()
    {
        // Arrange
        using var helper = new CliTestHelper();
        // Don't create solution structure

        // Act
        var exitCode = helper.RunCommand("module", "list", "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(1);
        helper.Error.Should().Contain("Cannot find Oqtane solution");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleList_NoTemplates_ShowsNoTemplatesMessage()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution(includeTemplates: false);

        // Act
        var exitCode = helper.RunCommand("module", "list", "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("No module templates found");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleList_SkipsInvalidManifests()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution(includeTemplates: false);
        helper.CreateTemplateDirectory("Internal", "Internal Template", validManifest: false);
        helper.CreateTemplateDirectory("External", "External Template", validManifest: true);

        // Act
        var exitCode = helper.RunCommand("module", "list", "--solution", helper.TestDirectory);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("external");
        helper.Output.Should().NotContain("internal");
    }

    [Fact(Skip = "Requires CLI refactoring to support programmatic invocation")]
    public void ModuleList_FromSubdirectory_FindsSolution()
    {
        // Arrange
        using var helper = new CliTestHelper();
        helper.CreateMockOqtaneSolution();
        var subDir = helper.CreateSubdirectory("SubDir1/SubDir2");

        // Act
        var exitCode = helper.RunCommand("module", "list", "--solution", subDir);

        // Assert
        exitCode.Should().Be(0);
        helper.Output.Should().Contain("Available Module Templates:");
    }
}
