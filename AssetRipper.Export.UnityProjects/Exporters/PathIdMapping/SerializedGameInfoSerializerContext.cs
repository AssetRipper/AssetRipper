using System.Text.Json.Serialization;

namespace AssetRipper.Library.Exporters.PathIdMapping;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SerializedGameInfo))]
internal sealed partial class SerializedGameInfoSerializerContext : JsonSerializerContext
{
}
