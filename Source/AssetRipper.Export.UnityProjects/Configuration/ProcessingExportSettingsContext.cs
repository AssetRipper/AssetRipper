using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProcessingSettings))]
[JsonSerializable(typeof(ExportSettings))]
internal sealed partial class ProcessingExportSettingsContext : JsonSerializerContext
{
}
