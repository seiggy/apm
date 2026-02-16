using System.CommandLine;
using System.CommandLine.Invocation;
using Spectre.Console;
using Apm.Cli.Dependencies;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands.Deps;

public static class DepsVerifyCommand
{
    public static Command Create()
    {
        var command = new Command("verify", Emoji.Replace(":check_mark_button: Verify installed dependencies"));
        command.SetHandler(ctx =>
        {
            ctx.ExitCode = Execute();
        });
        return command;
    }

    internal static int Execute()
    {
        try
        {
            var projectRoot = Directory.GetCurrentDirectory();
            var apmYmlPath = Path.Combine(projectRoot, "apm.yml");

            if (!File.Exists(apmYmlPath))
            {
                ConsoleHelpers.Error("No apm.yml found. Run 'apm init' first.");
                return 1;
            }

            // Verify declared dependencies are installed
            var (allInstalled, installed, missing) = Verifier.VerifyDependencies(projectRoot: projectRoot);

            // Verify lockfile matches
            var (lockMatch, lockMatched, lockMismatched) = Verifier.VerifyLockfile(projectRoot);

            // Display results
            var table = new Table()
                .Title(":check_mark_button: Dependency Verification")
                .Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("[bold cyan]Check[/]").NoWrap());
            table.AddColumn(new TableColumn("[bold cyan]Status[/]"));
            table.AddColumn(new TableColumn("[bold cyan]Details[/]"));

            // Declared deps check
            table.AddRow(
                "Declared dependencies",
                allInstalled ? "[green]:check_mark_button: Pass[/]" : "[red]:cross_mark: Fail[/]",
                allInstalled
                    ? $"[green]{installed.Count} installed[/]"
                    : $"[red]{missing.Count} missing, {installed.Count} installed[/]");

            // Lockfile check
            var lockfilePath = LockFile.GetLockfilePath(projectRoot);
            if (File.Exists(lockfilePath))
            {
                table.AddRow(
                    "Lockfile integrity",
                    lockMatch ? "[green]:check_mark_button: Pass[/]" : "[red]:cross_mark: Fail[/]",
                    lockMatch
                        ? $"[green]{lockMatched.Count} matched[/]"
                        : $"[red]{lockMismatched.Count} mismatched[/]");
            }
            else
            {
                table.AddRow("Lockfile integrity", "[yellow]:warning: Skip[/]", "[dim]No lockfile found[/]");
            }

            AnsiConsole.Write(table);

            // Show missing details
            if (missing.Count > 0)
            {
                AnsiConsole.WriteLine();
                ConsoleHelpers.Warning("Missing dependencies:");
                foreach (var dep in missing)
                    AnsiConsole.MarkupLine($"  :cross_mark: [red]{Markup.Escape(dep)}[/]");
                AnsiConsole.WriteLine();
                ConsoleHelpers.Info("Run 'apm install' to install missing dependencies", symbol: "bulb");
            }

            if (lockMismatched.Count > 0 && File.Exists(lockfilePath))
            {
                AnsiConsole.WriteLine();
                ConsoleHelpers.Warning("Lockfile mismatches:");
                foreach (var dep in lockMismatched)
                    AnsiConsole.MarkupLine($"  :warning: [yellow]{Markup.Escape(dep)}[/]");
            }

            return allInstalled && lockMatch ? 0 : 1;
        }
        catch (Exception e)
        {
            ConsoleHelpers.Error($"Error verifying dependencies: {e.Message}");
            return 1;
        }
    }
}
