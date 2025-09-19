using AssetRipper.Export.Configuration;
using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SerializedSettings))]
internal sealed partial class SerializedSettingsContext : JsonSerializerContext
{
}
