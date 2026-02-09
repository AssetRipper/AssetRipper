using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Shaders;

[JsonSerializable(typeof(TemplateJson))]
internal sealed partial class TemplateJsonSerializerContext : JsonSerializerContext
{
}
