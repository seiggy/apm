using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json.Nodes;
using Spectre.Console;
using Apm.Cli.Core;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands.Config;

public static class ConfigGetCommand
{
    public static Command Create()
    {
        var keyArg = new Argument<string?>("key", "Configuration key to retrieve")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };

        var command = new Command("get", Emoji.Replace(":open_book: Get configuration value"));
        command.AddArgument(keyArg);
        command.SetHandler(ctx =>
        {
            var key = ctx.ParseResult.GetValueForArgument(keyArg);
            ctx.ExitCode = Execute(key);
        });
        return command;
    }

    private static readonly HashSet<string> ValidKeys = ["auto-integrate"];

    internal static int Execute(string? key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            if (key == "auto-integrate")
            {
                var config = Configuration.GetConfig();
                var value = config.TryGetValue("auto_integrate", out var node) && node is not null
                    ? node.ToString()
                    : "true";
                AnsiConsole.MarkupLine($"auto-integrate: {Markup.Escape(value)}");
            }
            else
            {
                ConsoleHelpers.Error($"Unknown configuration key: '{key}'");
                ConsoleHelpers.Info($"Valid keys: {string.Join(", ", ValidKeys)}");
                return 1;
            }
        }
        else
        {
            // Show all config
            var config = Configuration.GetConfig();
            ConsoleHelpers.Info("APM Configuration:");
            foreach (var (k, value) in config)
            {
                var displayKey = k == "auto_integrate" ? "auto-integrate" : k;
                AnsiConsole.MarkupLine($"  {Markup.Escape(displayKey)}: {Markup.Escape(value?.ToString() ?? "")}");
            }
        }

        return 0;
    }
}
