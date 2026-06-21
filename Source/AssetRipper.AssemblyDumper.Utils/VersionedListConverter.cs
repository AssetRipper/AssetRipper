using AssetRipper.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AssetRipper.AssemblyDumper.Utils;

public class VersionedListConverter<T> : JsonConverter<VersionedList<T>>
{
	public override VersionedList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		JsonTypeInfo<T> jsonTypeInfo = options.GetTypeInfo<T>();

		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException();
		}

		VersionedList<T> dictionary = [];

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				return dictionary;
			}

			// Get the key.
			if (reader.TokenType != JsonTokenType.PropertyName)
			{
				throw new JsonException();
			}

			string? propertyName = reader.GetString();

			UnityVersion key;
			if (propertyName is null)
			{
				throw new JsonException("Unable to parse UnityVersion from null string.");
			}
			else
			{
				try
				{
					key = UnityVersion.Parse(propertyName);
				}
				catch (Exception e)
				{
					throw new JsonException($"Unable to convert \"{propertyName}\" to UnityVersion.", e);
				}
			}

			// Get the value.
			T? value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);

			// Add to dictionary.
			dictionary.Add(key, value);
		}

		throw new JsonException();
	}

	public override void Write(Utf8JsonWriter writer, VersionedList<T> dictionary, JsonSerializerOptions options)
	{
		JsonTypeInfo<T> jsonTypeInfo = options.GetTypeInfo<T>();

		writer.WriteStartObject();

		foreach ((UnityVersion key, T? value) in dictionary)
		{
			writer.WritePropertyName(key.ToString());
			if (value is null)
			{
				writer.WriteNullValue();
			}
			else
			{
				JsonSerializer.Serialize(writer, value, jsonTypeInfo);
			}
		}

		writer.WriteEndObject();
	}
}
