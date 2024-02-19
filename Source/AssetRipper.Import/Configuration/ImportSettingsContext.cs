using System.Text.Json.Serialization;

namespace AssetRipper.Import.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ImportSettings))]
internal sealed partial class ImportSettingsContext : JsonSerializerContext
{
}
