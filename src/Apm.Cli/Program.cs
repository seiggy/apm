using System.CommandLine;
using Spectre.Console;
using Apm.Cli.Commands;
using Apm.Cli.Commands.Config;
using Apm.Cli.Commands.Deps;

var rootCommand = new RootCommand("APM - The AI Package Manager");

rootCommand.AddCommand(InitCommand.Create());
rootCommand.AddCommand(InstallCommand.Create());
rootCommand.AddCommand(CompileCommand.Create());
rootCommand.AddCommand(RunCommand.Create());
rootCommand.AddCommand(PreviewCommand.Create());
rootCommand.AddCommand(ListCommand.Create());

var depsCommand = new Command("deps", Emoji.Replace(":clipboard: Manage APM package dependencies"));
depsCommand.AddCommand(DepsListCommand.Create());
depsCommand.AddCommand(DepsTreeCommand.Create());
depsCommand.AddCommand(DepsVerifyCommand.Create());
depsCommand.AddCommand(DepsUninstallCommand.Create());
rootCommand.AddCommand(depsCommand);

var configCommand = new Command("config", Emoji.Replace(":gear: Configure APM CLI"));
configCommand.AddCommand(ConfigGetCommand.Create());
configCommand.AddCommand(ConfigSetCommand.Create());
configCommand.AddCommand(ConfigShowCommand.Create());
rootCommand.AddCommand(configCommand);

return rootCommand.Invoke(args);
