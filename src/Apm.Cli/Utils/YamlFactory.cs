using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Apm.Cli.Utils;

/// <summary>
/// Centralized YAML serializer/deserializer factory for NativeAOT compatibility.
/// Uses StaticDeserializerBuilder/StaticSerializerBuilder with ApmYamlStaticContext
/// to avoid reflection-based code generation at runtime.
/// All model properties use explicit [YamlMember(Alias)] attributes, so NullNamingConvention
/// is used to avoid naming convention interference with aliases.
/// </summary>
internal static class YamlFactory
{
    /// <summary>Deserializer with camelCase naming convention (legacy name, uses explicit aliases).</summary>
    public static readonly IDeserializer CamelCaseDeserializer =
        new StaticDeserializerBuilder(new ApmYamlStaticContext())
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    /// <summary>Deserializer with underscore_case naming convention (legacy name, uses explicit aliases).</summary>
    public static readonly IDeserializer UnderscoreDeserializer =
        new StaticDeserializerBuilder(new ApmYamlStaticContext())
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    /// <summary>Serializer with underscore_case naming convention (legacy name, uses explicit aliases).</summary>
    public static readonly ISerializer UnderscoreSerializer =
        new StaticSerializerBuilder(new ApmYamlStaticContext())
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();

    /// <summary>Serializer with underscore_case naming, omitting null values.</summary>
    public static readonly ISerializer UnderscoreSerializerOmitNull =
        new StaticSerializerBuilder(new ApmYamlStaticContext())
            .WithNamingConvention(NullNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

    /// <summary>Serializer with underscore_case naming, preserving default values.</summary>
    public static readonly ISerializer UnderscoreSerializerPreserve =
        new StaticSerializerBuilder(new ApmYamlStaticContext())
            .WithNamingConvention(NullNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
            .Build();
}
