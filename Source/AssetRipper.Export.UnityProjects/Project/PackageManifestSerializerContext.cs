using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Project;

[JsonSerializable(typeof(PackageManifest))]
[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Serialization)]
internal partial class PackageManifestSerializerContext : JsonSerializerContext
{
}
