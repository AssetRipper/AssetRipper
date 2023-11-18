using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Project;

[JsonSerializable(typeof(PackageManifest))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class PackageManifestSerializerContext : JsonSerializerContext
{
}
