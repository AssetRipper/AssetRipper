using AssetRipper.Mining.PredefinedAssets;
using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(EngineResourceData?))]
internal partial class EngineResourceDataContext : JsonSerializerContext
{
}
