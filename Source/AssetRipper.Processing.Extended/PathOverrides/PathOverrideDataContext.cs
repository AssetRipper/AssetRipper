using System.Text.Json.Serialization;

namespace AssetRipper.Processing.Extended.PathOverrides;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PathOverrideData))]
public sealed partial class PathOverrideDataContext : JsonSerializerContext
{
}
