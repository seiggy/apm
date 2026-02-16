using System.CommandLine;
using System.CommandLine.Invocation;
using Spectre.Console;
using Apm.Cli.Core;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands;

public static class RunCommand
{
    public static Command Create()
    {
        var scriptArg = new Argument<string?>("script", "Script name to run")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };
        var paramOpt = new Option<string[]>(["--param", "-p"], "Parameter in format name=value")
        {
            AllowMultipleArgumentsPerToken = true,
        };

        var command = new Command("run", Emoji.Replace(":play_button: Run a script with parameters"));
        command.AddArgument(scriptArg);
        command.AddOption(paramOpt);
        command.SetHandler(ctx =>
        {
            var script = ctx.ParseResult.GetValueForArgument(scriptArg);
            var parameters = ctx.ParseResult.GetValueForOption(paramOpt);
            ctx.ExitCode = Execute(script, parameters);
        });
        return command;
    }

    internal static int Execute(string? scriptName, string[]? paramValues)
    {
        try
        {
            var scriptRunner = new ScriptRunner();

            // Default to 'start' script if none specified
            if (string.IsNullOrEmpty(scriptName))
            {
                var scripts = scriptRunner.ListScripts();
                if (scripts.TryGetValue("start", out _))
                {
                    scriptName = "start";
                }
                else
                {
                    ConsoleHelpers.Error("No script specified and no 'start' script defined in apm.yml");
                    ConsoleHelpers.Info("Available scripts:");

                    if (scripts.Count > 0)
                    {
                        var table = new Table { Border = TableBorder.None };
                        table.AddColumn(new TableColumn("Icon").NoWrap());
                        table.AddColumn(new TableColumn("Script").NoWrap());
                        table.AddColumn(new TableColumn("Command"));

                        foreach (var (name, command) in scripts)
                            table.AddRow("  ", $"[cyan]{Markup.Escape(name)}[/]", Markup.Escape(command));

                        AnsiConsole.Write(table);
                    }
                    else
                    {
                        ConsoleHelpers.Warning("No scripts defined in apm.yml");
                    }

                    return 1;
                }
            }

            // Parse parameters
            var parameters = new Dictionary<string, string>();
            if (paramValues is not null)
            {
                foreach (var p in paramValues)
                {
                    if (p.Contains('='))
                    {
                        var eqIdx = p.IndexOf('=');
                        var paramName = p[..eqIdx];
                        var value = p[(eqIdx + 1)..];
                        parameters[paramName] = value;
                        ConsoleHelpers.Echo($"  - {paramName}: {value}", color: "dim");
                    }
                }
            }

            // Execute the script
            var success = scriptRunner.RunScript(scriptName, parameters);

            if (!success)
            {
                ConsoleHelpers.Error("Script execution failed");
                return 1;
            }

            AnsiConsole.WriteLine();
            ConsoleHelpers.Success("Script executed successfully!", symbol: "sparkles");
            return 0;
        }
        catch (Exception e)
        {
            ConsoleHelpers.Error($"Error running script: {e.Message}");
            return 1;
        }
    }
}
