using System.Text.Json.Serialization;

namespace AssetRipper.Library.Exporters.Shaders
{
	[JsonSerializable(typeof(TemplateJson))]
	internal sealed partial class TemplateJsonSerializerContext : JsonSerializerContext
	{
	}
}
