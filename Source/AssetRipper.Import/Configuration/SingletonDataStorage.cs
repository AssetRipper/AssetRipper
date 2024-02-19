using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AssetRipper.Import.Configuration;

public sealed class SingletonDataStorage : DataStorage<DataInstance>
{
	public void Add(string key, string value)
	{
		Add(key, new StringDataInstance() { Value = value });
	}

	public bool TryGetStoredValue<T>(string key, [MaybeNullWhen(false)] out T value)
	{
		if (data.TryGetValue(key, out DataInstance? storedValue) && storedValue is DataInstance<T> instance)
		{
			value = instance.Value;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
}
public abstract class DataSerializer<T>
{
	public abstract T Deserialize(string text);
	public abstract string Serialize(T value);
	public abstract T CreateNew();
}
public sealed class StringDataSerializer : DataSerializer<string>
{
	public static StringDataSerializer Instance { get; } = new();

	public override string Deserialize(string text) => text;
	public override string Serialize(string value) => value;
	public override string CreateNew() => "";
}
public sealed class ParsableDataSerializer<T> : DataSerializer<T> where T : IParsable<T>, new()
{
	public static ParsableDataSerializer<T> Instance { get; } = new();

	public override T Deserialize(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return CreateNew();
		}

		// Forgiving parsing
		if (T.TryParse(text, null, out T? value))
		{
			return value;
		}
		else
		{
			return CreateNew();
		}
	}

	public override string Serialize(T value) => value.ToString() ?? "";
	public override T CreateNew() => new();
}
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
