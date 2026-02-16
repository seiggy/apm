using YamlDotNet.Serialization;

namespace Apm.Cli.Models;

/// <summary>
/// Strongly-typed model for .collection.yml YAML deserialization.
/// </summary>
public class CollectionManifestYaml
{
    [YamlMember(Alias = "id")]
    public string? Id { get; set; }

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "items")]
    public List<CollectionItemYaml>? Items { get; set; }

    [YamlMember(Alias = "tags")]
    public List<string>? Tags { get; set; }

    [YamlMember(Alias = "display")]
    public Dictionary<string, object>? Display { get; set; }
}

/// <summary>
/// Strongly-typed model for a single item in a collection manifest.
/// </summary>
public class CollectionItemYaml
{
    [YamlMember(Alias = "path")]
    public string? Path { get; set; }

    [YamlMember(Alias = "kind")]
    public string? Kind { get; set; }
}
