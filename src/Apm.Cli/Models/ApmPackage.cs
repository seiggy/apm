using Spectre.Console;
using Apm.Cli.Utils;
using YamlDotNet.Serialization;

namespace Apm.Cli.Models;

/// <summary>Represents an APM package with metadata.</summary>
public class ApmPackage
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = "";

    [YamlMember(Alias = "version")]
    public string Version { get; set; } = "";

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "author")]
    public string? Author { get; set; }

    [YamlMember(Alias = "license")]
    public string? License { get; set; }

    /// <summary>Source location (for dependencies).</summary>
    [YamlIgnore]
    public string? Source { get; set; }

    /// <summary>Resolved commit SHA (for dependencies).</summary>
    [YamlIgnore]
    public string? ResolvedCommit { get; set; }

    /// <summary>
    /// Dependencies by type. APM deps are stored as DependencyReference; other types as strings.
    /// </summary>
    [YamlIgnore]
    public Dictionary<string, List<object>>? Dependencies { get; set; }

    [YamlMember(Alias = "scripts")]
    public Dictionary<string, string>? Scripts { get; set; }

    /// <summary>Local path to the package directory.</summary>
    [YamlIgnore]
    public string? PackagePath { get; set; }

    /// <summary>Target agent: vscode, claude, or all.</summary>
    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    /// <summary>Package content type: instructions, skill, hybrid, or prompts.</summary>
    [YamlIgnore]
    public PackageContentType? Type { get; set; }

    /// <summary>Load APM package from apm.yml file.</summary>
    /// <exception cref="FileNotFoundException">If the file doesn't exist.</exception>
    /// <exception cref="ArgumentException">If the file is invalid or missing required fields.</exception>
    public static ApmPackage FromApmYml(string apmYmlPath)
    {
        if (!File.Exists(apmYmlPath))
            throw new FileNotFoundException($"apm.yml not found: {apmYmlPath}", apmYmlPath);

        string yamlContent;
        try
        {
            yamlContent = File.ReadAllText(apmYmlPath);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new ArgumentException($"Failed to read {apmYmlPath}: {ex.Message}", ex);
        }

        ApmManifest manifest;
        try
        {
            manifest = YamlFactory.UnderscoreDeserializer.Deserialize<ApmManifest>(yamlContent)
                       ?? throw new ArgumentException($"apm.yml must contain a YAML object");
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new ArgumentException($"Invalid YAML format in {apmYmlPath}: {ex.Message}", ex);
        }

        if (string.IsNullOrEmpty(manifest.Name))
            throw new ArgumentException("Missing required field 'name' in apm.yml");
        if (string.IsNullOrEmpty(manifest.Version))
            throw new ArgumentException("Missing required field 'version' in apm.yml");

        // Parse dependencies
        Dictionary<string, List<object>>? dependencies = null;
        if (manifest.Dependencies != null)
        {
            dependencies = new Dictionary<string, List<object>>();

            if (manifest.Dependencies.Apm is { } apmDeps)
            {
                var parsed = new List<object>();
                foreach (var depStr in apmDeps)
                {
                    if (string.IsNullOrEmpty(depStr)) continue;
                    try
                    {
                        parsed.Add(DependencyReference.Parse(depStr));
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Invalid APM dependency '{depStr}': {ex.Message}", ex);
                    }
                }
                dependencies["apm"] = parsed;
            }

            if (manifest.Dependencies.Mcp is { } mcpDeps)
            {
                dependencies["mcp"] = mcpDeps.Where(d => !string.IsNullOrEmpty(d)).Select(d => (object)d).ToList();
            }
        }

        // Parse package content type
        PackageContentType? pkgType = null;
        if (manifest.Type != null)
        {
            try
            {
                pkgType = PackageContentTypeExtensions.FromString(manifest.Type);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid 'type' field in apm.yml: {ex.Message}", ex);
            }
        }

        return new ApmPackage
        {
            Name = manifest.Name,
            Version = manifest.Version,
            Description = manifest.Description,
            Author = manifest.Author,
            License = manifest.License,
            Dependencies = dependencies,
            Scripts = manifest.Scripts,
            PackagePath = Path.GetDirectoryName(Path.GetFullPath(apmYmlPath)),
            Target = manifest.Target,
            Type = pkgType,
        };
    }

    /// <summary>Get list of APM dependencies.</summary>
    public List<DependencyReference> GetApmDependencies()
    {
        if (Dependencies == null || !Dependencies.TryGetValue("apm", out var apmDeps))
            return [];
        return apmDeps.OfType<DependencyReference>().ToList();
    }

    /// <summary>Get list of MCP dependencies (as strings).</summary>
    public List<string> GetMcpDependencies()
    {
        if (Dependencies == null || !Dependencies.TryGetValue("mcp", out var mcpDeps))
            return [];
        return mcpDeps.Select(d => d.ToString()!).ToList();
    }

    /// <summary>Check if this package has APM dependencies.</summary>
    public bool HasApmDependencies() => GetApmDependencies().Count > 0;
}

/// <summary>Result of APM package validation.</summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
    public ApmPackage? Package { get; set; }
    public PackageType? PackageTypeResult { get; set; }

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public void AddWarning(string warning) => Warnings.Add(warning);

    public bool HasIssues() => Errors.Count > 0 || Warnings.Count > 0;

    public string Summary()
    {
        if (IsValid && Warnings.Count == 0)
            return Emoji.Replace(":check_mark_button: Package is valid");
        if (IsValid)
            return Emoji.Replace($":warning: Package is valid with {Warnings.Count} warning(s)");
        return Emoji.Replace($":cross_mark: Package is invalid with {Errors.Count} error(s)");
    }
}
