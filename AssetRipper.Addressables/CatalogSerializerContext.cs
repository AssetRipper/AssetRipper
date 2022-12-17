using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

[JsonSourceGenerationOptions(WriteIndented = true)]//Source files are not indented
[JsonSerializable(typeof(Catalog))]
internal sealed partial class CatalogSerializerContext : JsonSerializerContext
{
}
