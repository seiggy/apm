using System.CommandLine;
using System.CommandLine.Invocation;
using Spectre.Console;
using Apm.Cli.Core;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands;

public static class PreviewCommand
{
    public static Command Create()
    {
        var scriptArg = new Argument<string?>("script", "Script name to preview")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };
        var paramOpt = new Option<string[]>(["--param", "-p"], "Parameter in format name=value")
        {
            AllowMultipleArgumentsPerToken = true,
        };

        var command = new Command("preview", Emoji.Replace(":eyes: Preview a script's compiled prompt files"));
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
                var allScripts = scriptRunner.ListScripts();
                if (allScripts.TryGetValue("start", out _))
                {
                    scriptName = "start";
                }
                else
                {
                    ConsoleHelpers.Error("No script specified and no 'start' script defined in apm.yml");
                    return 1;
                }
            }

            ConsoleHelpers.Info($"Previewing script: {scriptName}", symbol: "info");

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

            // Get the script command
            var scripts = scriptRunner.ListScripts();
            if (!scripts.TryGetValue(scriptName, out var command))
            {
                ConsoleHelpers.Error($"Script '{scriptName}' not found");
                return 1;
            }

            // Show original command
            ConsoleHelpers.Panel(command, title: ":page_facing_up: Original command", borderStyle: "blue");

            // Auto-compile prompts to show what would be executed
            var (compiledCommand, compiledFiles) = PreviewAutoCompilePrompts(
                command, parameters, scriptRunner);

            if (compiledFiles.Count > 0)
            {
                ConsoleHelpers.Panel(compiledCommand, title: ":high_voltage: Compiled command", borderStyle: "green");

                // Show compiled files
                var fileLines = compiledFiles
                    .Select(f =>
                    {
                        var stem = Path.GetFileNameWithoutExtension(
                            Path.GetFileNameWithoutExtension(f));
                        var compiledPath = Path.Combine(".apm", "compiled", $"{stem}.txt");
                        return $":page_facing_up: {compiledPath}";
                    });
                ConsoleHelpers.Panel(
                    string.Join("\n", fileLines),
                    title: ":file_folder: Compiled prompt files",
                    borderStyle: "cyan");
            }
            else
            {
                ConsoleHelpers.Panel(compiledCommand,
                    title: ":high_voltage: Command (no prompt compilation)",
                    borderStyle: "yellow");

                ConsoleHelpers.Panel(
                    "No .prompt.md files were compiled.\n\n" +
                    "APM only compiles files ending with '.prompt.md' extension.\n" +
                    "Other files are executed as-is by the runtime.",
                    title: ":information: Compilation Info",
                    borderStyle: "cyan");
            }

            AnsiConsole.WriteLine();
            ConsoleHelpers.Success(
                $"Preview complete! Use 'apm run {scriptName}' to execute.",
                symbol: "sparkles");
            return 0;
        }
        catch (Exception e)
        {
            ConsoleHelpers.Error($"Error previewing script: {e.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Preview-only prompt compilation: detects .prompt.md files in the command
    /// and simulates compilation without executing.
    /// </summary>
    private static (string CompiledCommand, List<string> CompiledFiles) PreviewAutoCompilePrompts(
        string command, Dictionary<string, string> parameters, ScriptRunner scriptRunner)
    {
        // Find .prompt.md file references in the command
        var promptFilePattern = new System.Text.RegularExpressions.Regex(@"\S+\.prompt\.md");
        var matches = promptFilePattern.Matches(command);
        var compiledFiles = new List<string>();
        var compiledCommand = command;

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var promptFile = match.Value;
            compiledFiles.Add(promptFile);

            // Build compiled path for display
            var stem = Path.GetFileNameWithoutExtension(
                Path.GetFileNameWithoutExtension(promptFile));
            var compiledPath = Path.Combine(".apm", "compiled", $"{stem}.txt");
            compiledCommand = compiledCommand.Replace(promptFile, compiledPath);
        }

        return (compiledCommand, compiledFiles);
    }
}
