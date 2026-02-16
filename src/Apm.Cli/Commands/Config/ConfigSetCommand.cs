using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json.Nodes;
using Apm.Cli.Core;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands.Config;

public static class ConfigSetCommand
{
    public static Command Create()
    {
        var keyArg = new Argument<string>("key", "Configuration key to set");
        var valueArg = new Argument<string>("value", "Value to set");

        var command = new Command("set", "✏️  Set configuration value");
        command.AddArgument(keyArg);
        command.AddArgument(valueArg);
        command.SetHandler(ctx =>
        {
            var key = ctx.ParseResult.GetValueForArgument(keyArg);
            var value = ctx.ParseResult.GetValueForArgument(valueArg);
            ctx.ExitCode = Execute(key, value);
        });
        return command;
    }

    private static readonly string[] TrueValues = ["true", "1", "yes"];
    private static readonly string[] FalseValues = ["false", "0", "no"];

    internal static int Execute(string key, string value)
    {
        if (key == "auto-integrate")
        {
            if (TrueValues.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                Configuration.UpdateConfig(new Dictionary<string, JsonNode?>
                {
                    ["auto_integrate"] = true
                });
                ConsoleHelpers.Success("Auto-integration enabled");
            }
            else if (FalseValues.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                Configuration.UpdateConfig(new Dictionary<string, JsonNode?>
                {
                    ["auto_integrate"] = false
                });
                ConsoleHelpers.Success("Auto-integration disabled");
            }
            else
            {
                ConsoleHelpers.Error($"Invalid value '{value}'. Use 'true' or 'false'.");
                return 1;
            }
        }
        else
        {
            ConsoleHelpers.Error($"Unknown configuration key: '{key}'");
            ConsoleHelpers.Info("Valid keys: auto-integrate");
            return 1;
        }

        return 0;
    }
}
