using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.PathIdMapping;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SerializedGameInfo))]
internal sealed partial class SerializedGameInfoSerializerContext : JsonSerializerContext
{
}
