using Apm.Cli.Models;
using Apm.Cli.Utils;

namespace Apm.Cli.Dependencies;

/// <summary>A resolved dependency with exact commit/version information.</summary>
public class LockedDependency
{
    public string RepoUrl { get; set; } = "";
    public string? Host { get; set; }
    public string? ResolvedCommit { get; set; }
    public string? ResolvedRef { get; set; }
    public string? Version { get; set; }
    public string? VirtualPath { get; set; }
    public bool IsVirtual { get; set; }
    public int Depth { get; set; } = 1;
    public string? ResolvedBy { get; set; }

    /// <summary>Returns unique key for this dependency.</summary>
    public string GetUniqueKey()
        => IsVirtual && !string.IsNullOrEmpty(VirtualPath)
            ? $"{RepoUrl}/{VirtualPath}"
            : RepoUrl;

    /// <summary>Create from a DependencyReference with resolution info.</summary>
    public static LockedDependency FromDependencyRef(
        DependencyReference depRef,
        string? resolvedCommit,
        int depth,
        string? resolvedBy) => new()
        {
            RepoUrl = depRef.RepoUrl,
            Host = depRef.Host,
            ResolvedCommit = resolvedCommit,
            ResolvedRef = depRef.Reference,
            VirtualPath = depRef.VirtualPath,
            IsVirtual = depRef.IsVirtual,
            Depth = depth,
            ResolvedBy = resolvedBy
        };
}

/// <summary>APM lock file for reproducible dependency resolution.</summary>
public class LockFile
{
    public string LockfileVersion { get; set; } = "1";
    public string GeneratedAt { get; set; } = DateTimeOffset.UtcNow.ToString("o");
    public string? ApmVersion { get; set; }
    public Dictionary<string, LockedDependency> Dependencies { get; set; } = new();

    public void AddDependency(LockedDependency dep)
        => Dependencies[dep.GetUniqueKey()] = dep;

    public LockedDependency? GetDependency(string key)
        => Dependencies.GetValueOrDefault(key);

    public bool HasDependency(string key) => Dependencies.ContainsKey(key);

    /// <summary>Get all dependencies sorted by depth then repo_url.</summary>
    public List<LockedDependency> GetAllDependencies()
        => Dependencies.Values
            .OrderBy(d => d.Depth)
            .ThenBy(d => d.RepoUrl)
            .ToList();

    /// <summary>Serialize to YAML string.</summary>
    public string ToYaml()
    {
        var doc = new LockFileDocument
        {
            LockfileVersion = LockfileVersion,
            GeneratedAt = GeneratedAt,
            ApmVersion = ApmVersion,
            Dependencies = GetAllDependencies().Select(d => new LockedDependencyYaml
            {
                RepoUrl = d.RepoUrl,
                Host = d.Host,
                ResolvedCommit = d.ResolvedCommit,
                ResolvedRef = d.ResolvedRef,
                Version = d.Version,
                VirtualPath = d.VirtualPath,
                IsVirtual = d.IsVirtual ? true : null,
                Depth = d.Depth != 1 ? d.Depth : null,
                ResolvedBy = d.ResolvedBy
            }).ToList()
        };

        return YamlFactory.UnderscoreSerializerOmitNull.Serialize(doc);
    }

    /// <summary>Deserialize from YAML string.</summary>
    public static LockFile FromYaml(string yamlStr)
    {
        var doc = YamlFactory.UnderscoreDeserializer.Deserialize<LockFileDocument>(yamlStr);
        if (doc == null)
            return new LockFile();

        var lockFile = new LockFile
        {
            LockfileVersion = doc.LockfileVersion,
            GeneratedAt = doc.GeneratedAt,
            ApmVersion = doc.ApmVersion
        };

        if (doc.Dependencies is not null)
        {
            foreach (var dep in doc.Dependencies)
            {
                lockFile.AddDependency(new LockedDependency
                {
                    RepoUrl = dep.RepoUrl,
                    Host = dep.Host,
                    ResolvedCommit = dep.ResolvedCommit,
                    ResolvedRef = dep.ResolvedRef,
                    Version = dep.Version,
                    VirtualPath = dep.VirtualPath,
                    IsVirtual = dep.IsVirtual ?? false,
                    Depth = dep.Depth ?? 1,
                    ResolvedBy = dep.ResolvedBy
                });
            }
        }

        return lockFile;
    }

    /// <summary>Write lock file to disk.</summary>
    public void Write(string path) => File.WriteAllText(path, ToYaml());

    /// <summary>Read lock file from disk. Returns null if not exists or corrupt.</summary>
    public static LockFile? Read(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return FromYaml(File.ReadAllText(path));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Load existing lock file or create a new one.</summary>
    public static LockFile LoadOrCreate(string path) => Read(path) ?? new LockFile();

    /// <summary>Create a lock file from installed packages.</summary>
    /// <param name="installedPackages">Tuples of (depRef, resolvedCommit, depth, resolvedBy).</param>
    /// <param name="dependencyGraph">The resolved DependencyGraph for additional metadata.</param>
    public static LockFile FromInstalledPackages(
        List<(DependencyReference DepRef, string? ResolvedCommit, int Depth, string? ResolvedBy)> installedPackages,
        DependencyGraph dependencyGraph)
    {
        var lockFile = new LockFile
        {
            ApmVersion = typeof(LockFile).Assembly.GetName().Version?.ToString() ?? "unknown"
        };

        foreach (var (depRef, resolvedCommit, depth, resolvedBy) in installedPackages)
        {
            lockFile.AddDependency(LockedDependency.FromDependencyRef(
                depRef, resolvedCommit, depth, resolvedBy));
        }

        return lockFile;
    }

    /// <summary>Save lock file to disk (alias for Write).</summary>
    public void Save(string path) => Write(path);

    /// <summary>Get the path to the lock file for a project.</summary>
    public static string GetLockfilePath(string projectRoot) => Path.Combine(projectRoot, "apm.lock");
}
