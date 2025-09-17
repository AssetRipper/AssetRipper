using AssetRipper.DocExtraction.DataStructures;
using AssetRipper.DocExtraction.MetaData;
using System.Text.Json.Serialization;

namespace AssetRipper.DocExtraction;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DocumentationFile))]
[JsonSerializable(typeof(HistoryFile))]
[JsonSerializable(typeof(FullNameRecord))]
public sealed partial class JsonSourceGenerationContext : JsonSerializerContext
{
}