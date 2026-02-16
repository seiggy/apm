using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Spectre.Console;
using Apm.Cli.Models;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands;

public static class InitCommand
{
    public static Command Create()
    {
        var nameArg = new Argument<string?>("name", "Project name")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };
        var yesOpt = new Option<bool>(["--yes", "-y"], "Skip confirmation prompts");

        var command = new Command("init", "🚀 Initialize a new APM project");
        command.AddArgument(nameArg);
        command.AddOption(yesOpt);
        command.SetHandler(ctx =>
        {
            var name = ctx.ParseResult.GetValueForArgument(nameArg);
            var yes = ctx.ParseResult.GetValueForOption(yesOpt);
            ctx.ExitCode = Execute(name, yes);
        });
        return command;
    }

    internal static int Execute(string? projectName, bool yes)
    {
        try
        {
            // Handle explicit current directory
            if (projectName == ".")
                projectName = null;

            string finalProjectName;

            if (!string.IsNullOrEmpty(projectName))
            {
                var projectDir = Path.GetFullPath(projectName);
                Directory.CreateDirectory(projectDir);
                Directory.SetCurrentDirectory(projectDir);
                ConsoleHelpers.Info($"Created project directory: {projectName}", symbol: "folder");
                finalProjectName = projectName;
            }
            else
            {
                finalProjectName = Path.GetFileName(Directory.GetCurrentDirectory());
            }

            // Check for existing apm.yml
            var apmYmlExists = File.Exists("apm.yml");

            if (apmYmlExists)
            {
                ConsoleHelpers.Warning("apm.yml already exists");

                if (!yes)
                {
                    if (!AnsiConsole.Confirm("Continue and overwrite?", defaultValue: false))
                    {
                        ConsoleHelpers.Info("Initialization cancelled.");
                        return 0;
                    }
                }
                else
                {
                    ConsoleHelpers.Info("--yes specified, overwriting apm.yml...");
                }
            }

            // Get project configuration
            Dictionary<string, string> config;
            if (!yes)
                config = InteractiveProjectSetup(finalProjectName);
            else
                config = GetDefaultConfig(finalProjectName);

            ConsoleHelpers.Success($"Initializing APM project: {config["name"]}", symbol: "rocket");

            CreateMinimalApmYml(config);

            ConsoleHelpers.Success("APM project initialized successfully!", symbol: "sparkles");

            // Display created files table
            var filesTable = ConsoleHelpers.CreateFilesTable(
                [("✨ apm.yml", "Project configuration")],
                title: "Created Files");
            AnsiConsole.Write(filesTable);

            AnsiConsole.WriteLine();

            // Next steps panel
            var nextSteps = new[]
            {
                "Install a runtime:       apm runtime setup copilot",
                "Add APM dependencies:    apm install <owner>/<repo>",
                "Compile agent context:   apm compile",
                "Run your first workflow: apm run start",
            };

            ConsoleHelpers.Panel(
                string.Join("\n", nextSteps.Select(s => $"• {s}")),
                title: "💡 Next Steps",
                borderStyle: "cyan");

            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.Error($"Error initializing project: {ex.Message}");
            return 1;
        }
    }

    private static Dictionary<string, string> InteractiveProjectSetup(string defaultName)
    {
        var autoAuthor = AutoDetectAuthor();
        var autoDescription = AutoDetectDescription(defaultName);

        AnsiConsole.MarkupLine("\n[blue]Setting up your APM project...[/]");
        AnsiConsole.MarkupLine("[dim]Press ^C at any time to quit.[/]\n");

        var name = AnsiConsole.Ask("Project name:", defaultName).Trim();
        var version = AnsiConsole.Ask("Version:", "1.0.0").Trim();
        var description = AnsiConsole.Ask("Description:", autoDescription).Trim();
        var author = AnsiConsole.Ask("Author:", autoAuthor).Trim();

        var summary = $"name: {name}\nversion: {version}\ndescription: {description}\nauthor: {author}";
        AnsiConsole.Write(new Panel(Markup.Escape(summary))
        {
            Header = new PanelHeader("About to create"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan"),
        });

        if (!AnsiConsole.Confirm("\nIs this OK?", defaultValue: true))
        {
            ConsoleHelpers.Info("Aborted.");
            Environment.Exit(0);
        }

        return new Dictionary<string, string>
        {
            ["name"] = name,
            ["version"] = version,
            ["description"] = description,
            ["author"] = author,
        };
    }

    private static Dictionary<string, string> GetDefaultConfig(string projectName)
    {
        return new Dictionary<string, string>
        {
            ["name"] = projectName,
            ["version"] = "1.0.0",
            ["description"] = AutoDetectDescription(projectName),
            ["author"] = AutoDetectAuthor(),
        };
    }

    private static void CreateMinimalApmYml(Dictionary<string, string> config)
    {
        var manifest = new ApmManifest
        {
            Name = config["name"],
            Version = config["version"],
            Description = config["description"],
            Author = config["author"],
            Dependencies = new ApmDependencies
            {
                Apm = [],
                Mcp = [],
            },
            Scripts = new Dictionary<string, string>(),
        };

        var yaml = YamlFactory.UnderscoreSerializerPreserve.Serialize(manifest);
        File.WriteAllText("apm.yml", yaml);
    }

    private static string AutoDetectAuthor()
    {
        try
        {
            var psi = new ProcessStartInfo("git", "config user.name")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi);
            if (proc is null) return "Developer";
            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit(5000);
            return proc.ExitCode == 0 && output.Length > 0 ? output : "Developer";
        }
        catch
        {
            return "Developer";
        }
    }

    private static string AutoDetectDescription(string projectName)
        => $"APM project for {projectName}";
}
