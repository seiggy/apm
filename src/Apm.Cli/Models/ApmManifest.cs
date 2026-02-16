using YamlDotNet.Serialization;

namespace Apm.Cli.Models;

/// <summary>
/// Strongly-typed model for apm.yml manifest files.
/// Used for both serialization and deserialization with YamlDotNet static generator.
/// </summary>
public class ApmManifest
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

    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    [YamlMember(Alias = "type")]
    public string? Type { get; set; }

    [YamlMember(Alias = "dependencies")]
    public ApmDependencies? Dependencies { get; set; }

    [YamlMember(Alias = "scripts")]
    public Dictionary<string, string>? Scripts { get; set; }

    [YamlMember(Alias = "compilation")]
    public CompilationSection? Compilation { get; set; }

    [YamlMember(Alias = "servers")]
    public List<string>? Servers { get; set; }
}

/// <summary>Dependencies section of apm.yml.</summary>
public class ApmDependencies
{
    [YamlMember(Alias = "apm")]
    public List<string>? Apm { get; set; }

    [YamlMember(Alias = "mcp")]
    public List<string>? Mcp { get; set; }
}

/// <summary>Compilation section of apm.yml.</summary>
public class CompilationSection
{
    [YamlMember(Alias = "output")]
    public string? Output { get; set; }

    [YamlMember(Alias = "chatmode")]
    public string? Chatmode { get; set; }

    [YamlMember(Alias = "resolve_links")]
    public bool? ResolveLinks { get; set; }

    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    [YamlMember(Alias = "strategy")]
    public string? Strategy { get; set; }

    [YamlMember(Alias = "source_attribution")]
    public bool? SourceAttribution { get; set; }

    [YamlMember(Alias = "single_file")]
    public bool? SingleFile { get; set; }

    [YamlMember(Alias = "placement")]
    public PlacementSection? Placement { get; set; }

    [YamlMember(Alias = "exclude")]
    public List<string>? Exclude { get; set; }
}

/// <summary>Placement subsection of compilation config.</summary>
public class PlacementSection
{
    [YamlMember(Alias = "min_instructions_per_file")]
    public int? MinInstructionsPerFile { get; set; }
}
