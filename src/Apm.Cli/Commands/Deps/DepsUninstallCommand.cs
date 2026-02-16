using System.CommandLine;
using System.CommandLine.Invocation;
using Spectre.Console;
using Apm.Cli.Models;
using Apm.Cli.Utils;

namespace Apm.Cli.Commands.Deps;

public static class DepsUninstallCommand
{
    public static Command Create()
    {
        var packagesArg = new Argument<string[]>("packages", "Packages to uninstall")
        {
            Arity = ArgumentArity.OneOrMore,
        };
        var dryRunOpt = new Option<bool>("--dry-run", "Show what would be removed without removing");

        var command = new Command("uninstall", "🗑️  Uninstall APM packages");
        command.AddArgument(packagesArg);
        command.AddOption(dryRunOpt);
        command.SetHandler(ctx =>
        {
            var packages = ctx.ParseResult.GetValueForArgument(packagesArg);
            var dryRun = ctx.ParseResult.GetValueForOption(dryRunOpt);
            ctx.ExitCode = Execute(packages, dryRun);
        });
        return command;
    }

    internal static int Execute(string[] packages, bool dryRun)
    {
        try
        {
            var apmYmlPath = Path.Combine(Directory.GetCurrentDirectory(), "apm.yml");
            if (!File.Exists(apmYmlPath))
            {
                ConsoleHelpers.Error("No apm.yml found. Run 'apm init' first.");
                return 1;
            }

            if (packages.Length == 0)
            {
                ConsoleHelpers.Error("No packages specified. Specify packages to uninstall.");
                return 1;
            }

            ConsoleHelpers.Info($"Uninstalling {packages.Length} package(s)...", symbol: "bulb");

            // Read current apm.yml
            ApmManifest manifest;
            try
            {
                var yamlContent = File.ReadAllText(apmYmlPath);
                manifest = YamlFactory.UnderscoreDeserializer.Deserialize<ApmManifest>(yamlContent) ?? new();
            }
            catch (Exception e)
            {
                ConsoleHelpers.Error($"Failed to read apm.yml: {e.Message}");
                return 1;
            }

            // Ensure dependencies structure exists
            manifest.Dependencies ??= new ApmDependencies();
            manifest.Dependencies.Apm ??= [];
            var currentDeps = manifest.Dependencies.Apm;

            var currentDepStrings = currentDeps.ToList();
            var packagesToRemove = new List<string>();
            var packagesNotFound = new List<string>();

            foreach (var package in packages)
            {
                if (!package.Contains('/'))
                {
                    ConsoleHelpers.Error($"Invalid package format: {package}. Use 'owner/repo' format.");
                    continue;
                }

                if (currentDepStrings.Contains(package))
                {
                    packagesToRemove.Add(package);
                    ConsoleHelpers.Info($"✓ {package} - found in apm.yml");
                }
                else
                {
                    packagesNotFound.Add(package);
                    ConsoleHelpers.Warning($"✗ {package} - not found in apm.yml");
                }
            }

            if (packagesToRemove.Count == 0)
            {
                ConsoleHelpers.Warning("No packages found in apm.yml to remove");
                return 0;
            }

            if (dryRun)
            {
                ConsoleHelpers.Info($"Dry run: Would remove {packagesToRemove.Count} package(s):");
                foreach (var pkg in packagesToRemove)
                {
                    ConsoleHelpers.Info($"  - {pkg} from apm.yml");
                    var apmModulesDir = Path.Combine(Directory.GetCurrentDirectory(), "apm_modules");
                    try
                    {
                        var depRef = DependencyReference.Parse(pkg);
                        var pkgPath = depRef.GetInstallPath(apmModulesDir);
                        if (Directory.Exists(pkgPath))
                            ConsoleHelpers.Info($"  - {pkg} from apm_modules/");
                    }
                    catch
                    {
                        var parts = pkg.Split('/');
                        var pkgPath = Path.Combine([apmModulesDir, .. parts]);
                        if (Directory.Exists(pkgPath))
                            ConsoleHelpers.Info($"  - {pkg} from apm_modules/");
                    }
                }
                ConsoleHelpers.Success("Dry run complete - no changes made", symbol: "sparkles");
                return 0;
            }

            // Remove packages from apm.yml
            foreach (var package in packagesToRemove)
            {
                currentDeps.Remove(package);
                ConsoleHelpers.Info($"Removed {package} from apm.yml");
            }

            // Write back to apm.yml
            try
            {
                File.WriteAllText(apmYmlPath, YamlFactory.UnderscoreSerializerOmitNull.Serialize(manifest));
                ConsoleHelpers.Success($"Updated apm.yml (removed {packagesToRemove.Count} package(s))", symbol: "sparkles");
            }
            catch (Exception e)
            {
                ConsoleHelpers.Error($"Failed to write apm.yml: {e.Message}");
                return 1;
            }

            // Remove packages from apm_modules/
            var apmModules = Path.Combine(Directory.GetCurrentDirectory(), "apm_modules");
            var removedFromModules = 0;

            if (Directory.Exists(apmModules))
            {
                foreach (var package in packagesToRemove)
                {
                    string packagePath;
                    try
                    {
                        var depRef = DependencyReference.Parse(package);
                        packagePath = depRef.GetInstallPath(apmModules);
                    }
                    catch
                    {
                        var parts = package.Split('/');
                        packagePath = Path.Combine([apmModules, .. parts]);
                    }

                    if (Directory.Exists(packagePath))
                    {
                        try
                        {
                            Directory.Delete(packagePath, recursive: true);
                            ConsoleHelpers.Info($"✓ Removed {package} from apm_modules/");
                            removedFromModules++;

                            // Cleanup empty parent directories up to apm_modules/
                            var parent = Directory.GetParent(packagePath);
                            while (parent != null && parent.FullName != apmModules && parent.Exists)
                            {
                                try
                                {
                                    if (!parent.EnumerateFileSystemInfos().Any())
                                    {
                                        parent.Delete();
                                        parent = parent.Parent;
                                    }
                                    else break;
                                }
                                catch { break; }
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleHelpers.Error($"✗ Failed to remove {package} from apm_modules/: {e.Message}");
                        }
                    }
                    else
                    {
                        ConsoleHelpers.Warning($"Package {package} not found in apm_modules/");
                    }
                }
            }

            // Final summary
            var summaryLines = new List<string> { $"Removed {packagesToRemove.Count} package(s) from apm.yml" };
            if (removedFromModules > 0)
                summaryLines.Add($"Removed {removedFromModules} package(s) from apm_modules/");

            ConsoleHelpers.Success("Uninstall complete: " + string.Join(", ", summaryLines), symbol: "sparkles");

            if (packagesNotFound.Count > 0)
                ConsoleHelpers.Warning($"Note: {packagesNotFound.Count} package(s) were not found in apm.yml");

            return 0;
        }
        catch (Exception e)
        {
            ConsoleHelpers.Error($"Error uninstalling packages: {e.Message}");
            return 1;
        }
    }
}
