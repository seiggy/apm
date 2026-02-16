using Apm.Cli.Models;
using Apm.Cli.Utils;

namespace Apm.Cli.Dependencies;

/// <summary>Represents a single item in a collection manifest.</summary>
public class CollectionItem
{
    /// <summary>Relative path to the file (e.g., "prompts/code-review.prompt.md").</summary>
    public string Path { get; set; } = "";

    /// <summary>Type of primitive (e.g., "prompt", "instruction", "chat-mode").</summary>
    public string Kind { get; set; } = "";

    private static readonly Dictionary<string, string> KindToSubdir = new(StringComparer.OrdinalIgnoreCase)
    {
        ["prompt"] = "prompts",
        ["instruction"] = "instructions",
        ["chat-mode"] = "chatmodes",
        ["chatmode"] = "chatmodes",
        ["agent"] = "agents",
        ["context"] = "contexts"
    };

    /// <summary>Get the .apm subdirectory for this item based on its kind.</summary>
    public string Subdirectory =>
        KindToSubdir.GetValueOrDefault(Kind.ToLowerInvariant(), "prompts");
}

/// <summary>Represents a parsed collection manifest (.collection.yml).</summary>
public class CollectionManifest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<CollectionItem> Items { get; set; } = [];
    public List<string>? Tags { get; set; }
    public Dictionary<string, object>? Display { get; set; }

    /// <summary>Get the number of items in this collection.</summary>
    public int ItemCount => Items.Count;

    /// <summary>Get all items of a specific kind.</summary>
    public List<CollectionItem> GetItemsByKind(string kind)
        => Items.Where(i => i.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)).ToList();
}

/// <summary>Parser for APM collection manifest files (.collection.yml).</summary>
public static class CollectionParser
{
    /// <summary>
    /// Parse a collection YAML manifest.
    /// </summary>
    /// <param name="content">Raw YAML content as bytes.</param>
    /// <returns>Parsed and validated collection manifest.</returns>
    /// <exception cref="ArgumentException">If the YAML is invalid or missing required fields.</exception>
    public static CollectionManifest ParseCollectionYml(byte[] content)
    {
        CollectionManifestYaml collectionYaml;
        try
        {
            var yamlStr = System.Text.Encoding.UTF8.GetString(content);
            collectionYaml = YamlFactory.UnderscoreDeserializer.Deserialize<CollectionManifestYaml>(yamlStr)
                   ?? throw new ArgumentException("Collection YAML must be a dictionary");
        }
        catch (YamlDotNet.Core.YamlException e)
        {
            throw new ArgumentException($"Invalid YAML format: {e.Message}", e);
        }

        // Validate required fields
        var missingFields = new List<string>();
        if (string.IsNullOrEmpty(collectionYaml.Id)) missingFields.Add("id");
        if (string.IsNullOrEmpty(collectionYaml.Name)) missingFields.Add("name");
        if (string.IsNullOrEmpty(collectionYaml.Description)) missingFields.Add("description");
        if (collectionYaml.Items is null || collectionYaml.Items.Count == 0)
        {
            if (collectionYaml.Items is null) missingFields.Add("items");
        }

        if (missingFields.Count > 0)
            throw new ArgumentException($"Collection manifest missing required fields: {string.Join(", ", missingFields)}");

        if (collectionYaml.Items!.Count == 0)
            throw new ArgumentException("Collection must contain at least one item");

        // Validate and convert items
        var items = new List<CollectionItem>();
        for (var idx = 0; idx < collectionYaml.Items.Count; idx++)
        {
            var item = collectionYaml.Items[idx];
            if (string.IsNullOrEmpty(item.Path))
                throw new ArgumentException($"Collection item {idx} missing required field 'path'");
            if (string.IsNullOrEmpty(item.Kind))
                throw new ArgumentException($"Collection item {idx} missing required field 'kind'");

            items.Add(new CollectionItem
            {
                Path = item.Path,
                Kind = item.Kind
            });
        }

        return new CollectionManifest
        {
            Id = collectionYaml.Id!,
            Name = collectionYaml.Name!,
            Description = collectionYaml.Description!,
            Items = items,
            Tags = collectionYaml.Tags,
            Display = collectionYaml.Display
        };
    }
}
