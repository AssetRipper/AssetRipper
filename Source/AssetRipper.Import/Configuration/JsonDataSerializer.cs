using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AssetRipper.Import.Configuration;

public sealed class JsonDataSerializer<T> : DataSerializer<T> where T : new()
{
	private readonly JsonTypeInfo<T> typeInfo;

	public JsonDataSerializer(JsonTypeInfo<T> typeInfo)
	{
		this.typeInfo = typeInfo;
	}

	public override T Deserialize(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return CreateNew();
		}

		// Forgiving parsing
		try
		{
			return JsonSerializer.Deserialize(text, typeInfo) ?? CreateNew();
		}
		catch
		{
			return CreateNew();
		}
	}

	public override string Serialize(T value)
	{
		string result = JsonSerializer.Serialize(value, typeInfo);
		return result is "null" ? "" : result;
	}

	public override T CreateNew() => new();
}
