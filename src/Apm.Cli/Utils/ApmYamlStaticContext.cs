using Apm.Cli.Models;
using YamlDotNet.Serialization;

namespace Apm.Cli.Utils;

/// <summary>
/// Static YAML context for NativeAOT compatibility.
/// Registers all types used in YAML serialization/deserialization across the CLI.
/// </summary>
[YamlStaticContext]
[YamlSerializable(typeof(ApmManifest))]
[YamlSerializable(typeof(ApmDependencies))]
[YamlSerializable(typeof(CompilationSection))]
[YamlSerializable(typeof(PlacementSection))]
[YamlSerializable(typeof(PrimitiveFrontmatter))]
[YamlSerializable(typeof(PromptFrontmatter))]
[YamlSerializable(typeof(SkillFileFrontmatter))]
[YamlSerializable(typeof(SkillMetadataSection))]
[YamlSerializable(typeof(LockFileDocument))]
[YamlSerializable(typeof(LockedDependencyYaml))]
[YamlSerializable(typeof(CollectionManifestYaml))]
[YamlSerializable(typeof(CollectionItemYaml))]
public partial class ApmYamlStaticContext : StaticContext
{
}
