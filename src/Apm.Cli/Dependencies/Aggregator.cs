using System.Text.RegularExpressions;
using Apm.Cli.Models;
using Apm.Cli.Primitives;
using Apm.Cli.Utils;

namespace Apm.Cli.Dependencies;

/// <summary>
/// Workflow dependency aggregator for APM-CLI.
/// Scans prompt files for MCP dependencies following VSCode's .github/prompts convention.
/// </summary>
public static class Aggregator
{
    private static readonly Regex FrontmatterRegex = new(
        @"^---\s*\r?\n(.*?)\r?\n---",
        RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Scan all .prompt.md workflow files for MCP dependencies.
    /// </summary>
    /// <returns>A set of unique MCP server names from all workflows.</returns>
    public static HashSet<string> ScanWorkflowsForDependencies()
    {
        string[] promptPatterns =
        [
            "**/.github/prompts/*.prompt.md",
            "**/*.prompt.md"
        ];

        var workflows = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pattern in promptPatterns)
        {
            foreach (var file in GlobFiles(".", pattern))
                workflows.Add(Path.GetFullPath(file));
        }

        var allServers = new HashSet<string>();
        foreach (var workflowFile in workflows)
        {
            try
            {
                var content = File.ReadAllText(workflowFile);
                var match = FrontmatterRegex.Match(content);
                if (!match.Success) continue;

                var metadata = YamlFactory.CamelCaseDeserializer.Deserialize<PromptFrontmatter>(match.Groups[1].Value);
                if (metadata == null) continue;

                if (metadata.Mcp is { } mcpList)
                {
                    foreach (var server in mcpList)
                    {
                        if (!string.IsNullOrEmpty(server))
                            allServers.Add(server);
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleHelpers.Warning($"Error processing {workflowFile}: {e.Message}");
            }
        }

        return allServers;
    }

    /// <summary>
    /// Extract all MCP servers from workflows into apm.yml.
    /// </summary>
    /// <returns>Tuple of (success, list of servers added).</returns>
    public static (bool Success, List<string> Servers) SyncWorkflowDependencies(string outputFile = "apm.yml")
    {
        var allServers = ScanWorkflowsForDependencies();
        var sortedServers = allServers.OrderBy(s => s).ToList();

        var apmConfig = new ApmManifest
        {
            Version = "1.0",
            Servers = sortedServers
        };

        try
        {
            File.WriteAllText(outputFile, YamlFactory.UnderscoreSerializer.Serialize(apmConfig));
            return (true, sortedServers);
        }
        catch (Exception e)
        {
            ConsoleHelpers.Error($"Error writing to {outputFile}: {e.Message}");
            return (false, []);
        }
    }

    private static IEnumerable<string> GlobFiles(string baseDir, string pattern)
    {
        var searchPattern = Path.GetFileName(pattern);
        try
        {
            return Directory.EnumerateFiles(baseDir, searchPattern, SearchOption.AllDirectories);
        }
        catch
        {
            return [];
        }
    }
}
