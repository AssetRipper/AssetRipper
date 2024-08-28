using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.Import.Configuration;

public sealed class UnityVersionJsonConverter : JsonConverter<UnityVersion>
{
	public override UnityVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return UnityVersion.Parse(reader.GetString() ?? throw new JsonException("String was read as null"));
	}

	public override void Write(Utf8JsonWriter writer, UnityVersion value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
