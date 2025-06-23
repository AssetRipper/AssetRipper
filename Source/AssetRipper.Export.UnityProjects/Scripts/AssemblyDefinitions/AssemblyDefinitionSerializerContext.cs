using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Scripts.AssemblyDefinitions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AssemblyDefinitionAsset))]
internal sealed partial class AssemblyDefinitionSerializerContext : JsonSerializerContext
{
}
