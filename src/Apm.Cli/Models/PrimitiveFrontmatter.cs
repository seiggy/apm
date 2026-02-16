using YamlDotNet.Serialization;

namespace Apm.Cli.Models;

/// <summary>
/// Strongly-typed model for primitive YAML frontmatter in .md files.
/// Used by PrimitiveParser for chatmodes, instructions, contexts, agents, etc.
/// </summary>
public class PrimitiveFrontmatter
{
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "applyTo")]
    public string? ApplyTo { get; set; }

    [YamlMember(Alias = "author")]
    public string? Author { get; set; }

    [YamlMember(Alias = "version")]
    public string? Version { get; set; }
}

/// <summary>
/// Frontmatter for .prompt.md files, extending PrimitiveFrontmatter with prompt-specific fields.
/// Used by Aggregator, WorkflowParser, and CommandIntegrator.
/// </summary>
public class PromptFrontmatter : PrimitiveFrontmatter
{
    [YamlMember(Alias = "mcp")]
    public List<string>? Mcp { get; set; }

    [YamlMember(Alias = "input")]
    public List<string>? Input { get; set; }

    [YamlMember(Alias = "llm")]
    public string? Llm { get; set; }

    [YamlMember(Alias = "allowedTools")]
    public string? AllowedTools { get; set; }

    [YamlMember(Alias = "allowed-tools")]
    public string? AllowedToolsHyphen { get; set; }

    [YamlMember(Alias = "model")]
    public string? Model { get; set; }

    [YamlMember(Alias = "argumentHint")]
    public string? ArgumentHint { get; set; }

    [YamlMember(Alias = "argument-hint")]
    public string? ArgumentHintHyphen { get; set; }

    /// <summary>Effective allowed-tools value (camelCase or hyphen format).</summary>
    [YamlIgnore]
    public string? EffectiveAllowedTools => AllowedTools ?? AllowedToolsHyphen;

    /// <summary>Effective argument-hint value (camelCase or hyphen format).</summary>
    [YamlIgnore]
    public string? EffectiveArgumentHint => ArgumentHint ?? ArgumentHintHyphen;
}

/// <summary>
/// Frontmatter for SKILL.md files with APM metadata section.
/// Used by SkillIntegrator to extract package metadata.
/// </summary>
public class SkillFileFrontmatter : PrimitiveFrontmatter
{
    [YamlMember(Alias = "metadata")]
    public SkillMetadataSection? Metadata { get; set; }
}

/// <summary>
/// Nested metadata section in SKILL.md frontmatter containing APM package info.
/// </summary>
public class SkillMetadataSection
{
    [YamlMember(Alias = "apm_version")]
    public string? ApmVersion { get; set; }

    [YamlMember(Alias = "apm_commit")]
    public string? ApmCommit { get; set; }

    [YamlMember(Alias = "apm_package")]
    public string? ApmPackage { get; set; }

    [YamlMember(Alias = "apm_content_hash")]
    public string? ApmContentHash { get; set; }
}
