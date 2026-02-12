using System.CommandLine;

var rootCommand = new RootCommand("Oqtane CLI - Module and theme scaffolding tool");

var moduleCommand = new Command("module", "Create and manage Oqtane modules");
rootCommand.Subcommands.Add(moduleCommand);

// Placeholder: module create and module list will be added in later phases
var createCommand = new Command("create", "Create a new Oqtane module");
moduleCommand.Subcommands.Add(createCommand);

createCommand.SetAction(_ =>
{
    Console.WriteLine("Module create is not yet implemented. See Issue #3 (Phase 1: CLI command parsing).");
});

return rootCommand.Parse(args).Invoke();
