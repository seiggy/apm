using Apm.Cli.Models;
using Apm.Cli.Utils;

namespace Apm.Cli.Compilation;

/// <summary>Configuration for AGENTS.md compilation.</summary>
public class CompilationConfig
{
    public string OutputPath { get; set; } = "AGENTS.md";
    public string? Chatmode { get; set; }
    public bool ResolveLinks { get; set; } = true;
    public bool DryRun { get; set; }
    public bool WithConstitution { get; set; } = true;

    /// <summary>
    /// Compilation target: "vscode", "agents", "claude", or "all".
    /// </summary>
    public string Target { get; set; } = "all";

    /// <summary>Compilation strategy: "distributed" or "single-file".</summary>
    public string Strategy { get; set; } = "distributed";

    /// <summary>Force single-file mode.</summary>
    public bool SingleAgents { get; set; }

    /// <summary>Show source attribution and conflicts.</summary>
    public bool Trace { get; set; }

    /// <summary>Ignore dependencies, compile only local primitives.</summary>
    public bool LocalOnly { get; set; }

    /// <summary>Show context optimizer analysis and metrics.</summary>
    public bool Debug { get; set; }

    /// <summary>Minimum instructions per AGENTS.md file (Minimal Context Principle).</summary>
    public int MinInstructionsPerFile { get; set; } = 1;

    /// <summary>Include source file comments.</summary>
    public bool SourceAttribution { get; set; } = true;

    /// <summary>Remove orphaned AGENTS.md files.</summary>
    public bool CleanOrphaned { get; set; }

    /// <summary>Glob patterns for directories to exclude during compilation.</summary>
    public List<string> Exclude { get; set; } = [];

    /// <summary>Apply CLI flag precedence after initialization.</summary>
    public void ApplyFlagPrecedence()
    {
        if (SingleAgents)
            Strategy = "single-file";
    }

    /// <summary>
    /// Create configuration from apm.yml with command-line overrides.
    /// </summary>
    public static CompilationConfig FromApmYml(Dictionary<string, object?>? overrides = null)
    {
        var config = new CompilationConfig();

        try
        {
            var apmYmlPath = Path.Combine(Directory.GetCurrentDirectory(), "apm.yml");
            if (File.Exists(apmYmlPath))
            {
                var yamlContent = File.ReadAllText(apmYmlPath);

                ApmManifest? manifest = null;
                try
                {
                    manifest = YamlFactory.UnderscoreDeserializer.Deserialize<ApmManifest>(yamlContent);
                }
                catch
                {
                    // Deserialization may fail due to type mismatches (e.g., exclude as string vs list)
                }

                if (manifest?.Compilation is { } compilation)
                {
                    if (compilation.Output != null)
                        config.OutputPath = compilation.Output;
                    if (compilation.Chatmode != null)
                        config.Chatmode = compilation.Chatmode;
                    if (compilation.ResolveLinks.HasValue)
                        config.ResolveLinks = compilation.ResolveLinks.Value;
                    if (compilation.Target != null)
                        config.Target = compilation.Target;
                    if (compilation.Strategy != null)
                        config.Strategy = compilation.Strategy;
                    if (compilation.SourceAttribution.HasValue)
                        config.SourceAttribution = compilation.SourceAttribution.Value;

                    // Legacy single_file support
                    if (compilation.SingleFile == true)
                    {
                        config.Strategy = "single-file";
                        config.SingleAgents = true;
                    }

                    // Placement settings
                    if (compilation.Placement?.MinInstructionsPerFile is { } minInst)
                        config.MinInstructionsPerFile = minInst;

                    // Exclude patterns
                    if (compilation.Exclude is { Count: > 0 } excludeList)
                        config.Exclude = excludeList;
                }

                // Handle single-string exclude: "exclude: pattern"
                // YamlDotNet typed deserialization can't auto-convert a scalar to List<string>,
                // so we fall back to raw text extraction when typed parsing didn't capture it.
                if (config.Exclude.Count == 0)
                {
                    var singleExclude = ExtractSingleExcludeValue(yamlContent);
                    if (singleExclude != null)
                        config.Exclude = [singleExclude];
                }
            }
        }
        catch (Exception)
        {
            // If config loading fails, use defaults
        }

        // Apply command-line overrides (highest priority)
        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
            {
                if (value is null) continue;
                ApplyOverride(config, key, value);
            }
        }

        config.ApplyFlagPrecedence();
        return config;
    }

    private static void ApplyOverride(CompilationConfig config, string key, object value)
    {
        switch (key)
        {
            case "output_path": config.OutputPath = value.ToString()!; break;
            case "chatmode": config.Chatmode = value.ToString(); break;
            case "resolve_links": config.ResolveLinks = Convert.ToBoolean(value); break;
            case "dry_run": config.DryRun = Convert.ToBoolean(value); break;
            case "with_constitution": config.WithConstitution = Convert.ToBoolean(value); break;
            case "target": config.Target = value.ToString()!; break;
            case "strategy": config.Strategy = value.ToString()!; break;
            case "single_agents": config.SingleAgents = Convert.ToBoolean(value); break;
            case "trace": config.Trace = Convert.ToBoolean(value); break;
            case "local_only": config.LocalOnly = Convert.ToBoolean(value); break;
            case "debug": config.Debug = Convert.ToBoolean(value); break;
            case "min_instructions_per_file": config.MinInstructionsPerFile = Convert.ToInt32(value); break;
            case "source_attribution": config.SourceAttribution = Convert.ToBoolean(value); break;
            case "clean_orphaned": config.CleanOrphaned = Convert.ToBoolean(value); break;
            case "exclude":
                if (value is List<string> list) config.Exclude = list;
                break;
        }
    }

    /// <summary>
    /// Extract single-string exclude value from raw YAML when typed deserialization returns null.
    /// YamlDotNet cannot auto-convert a YAML scalar to List&lt;string&gt;.
    /// </summary>
    private static string? ExtractSingleExcludeValue(string yamlContent)
    {
        foreach (var line in yamlContent.Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("exclude:", StringComparison.Ordinal))
            {
                var value = trimmed["exclude:".Length..].Trim().Trim('"', '\'');
                // Only accept single values (not list markers or empty)
                if (!string.IsNullOrEmpty(value) && !value.StartsWith('-') && !value.StartsWith('['))
                    return value;
            }
        }
        return null;
    }
}
