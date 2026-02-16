using YamlDotNet.Serialization;

namespace Apm.Cli.Models;

/// <summary>
/// Strongly-typed model for apm.lock YAML serialization/deserialization.
/// </summary>
public class LockFileDocument
{
    [YamlMember(Alias = "lockfile_version")]
    public string LockfileVersion { get; set; } = "1";

    [YamlMember(Alias = "generated_at")]
    public string GeneratedAt { get; set; } = "";

    [YamlMember(Alias = "apm_version")]
    public string? ApmVersion { get; set; }

    [YamlMember(Alias = "dependencies")]
    public List<LockedDependencyYaml>? Dependencies { get; set; }
}

/// <summary>
/// Strongly-typed model for a single locked dependency entry in apm.lock.
/// </summary>
public class LockedDependencyYaml
{
    [YamlMember(Alias = "repo_url")]
    public string RepoUrl { get; set; } = "";

    [YamlMember(Alias = "host")]
    public string? Host { get; set; }

    [YamlMember(Alias = "resolved_commit")]
    public string? ResolvedCommit { get; set; }

    [YamlMember(Alias = "resolved_ref")]
    public string? ResolvedRef { get; set; }

    [YamlMember(Alias = "version")]
    public string? Version { get; set; }

    [YamlMember(Alias = "virtual_path")]
    public string? VirtualPath { get; set; }

    [YamlMember(Alias = "is_virtual")]
    public bool? IsVirtual { get; set; }

    [YamlMember(Alias = "depth")]
    public int? Depth { get; set; }

    [YamlMember(Alias = "resolved_by")]
    public string? ResolvedBy { get; set; }
}
