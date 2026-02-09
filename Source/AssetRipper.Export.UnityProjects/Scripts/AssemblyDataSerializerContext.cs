using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Scripts;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(AssemblyDataFile))]
internal sealed partial class AssemblyDataSerializerContext : JsonSerializerContext
{
}
