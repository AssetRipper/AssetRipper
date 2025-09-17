using AssetRipper.Primitives;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.AssemblyDumper.Utils;

public class VersionedListConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(VersionedList<>);
	}

	public override JsonConverter CreateConverter(
		Type type,
		JsonSerializerOptions options)
	{
		Type valueType = type.GetGenericArguments()[0];

		JsonConverter converter = (JsonConverter)Activator.CreateInstance(
			typeof(VersionedListConverterInner<>).MakeGenericType([valueType]),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: [options],
			culture: null)!;

		return converter;
	}

	private class VersionedListConverterInner<TValue> : JsonConverter<VersionedList<TValue>>
	{
		private readonly JsonConverter<TValue?> _valueConverter;
		private readonly Type _valueType;

		public VersionedListConverterInner(JsonSerializerOptions options)
		{
			// For performance, use the existing converter if available.
			_valueConverter = (JsonConverter<TValue?>)options
				.GetConverter(typeof(TValue?));

			// Cache the key and value types.
			_valueType = typeof(TValue);
		}

		public override VersionedList<TValue> Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException();
			}

			VersionedList<TValue> dictionary = [];

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
				TValue? value;
				if (_valueConverter != null)
				{
					reader.Read();
					value = _valueConverter.Read(ref reader, _valueType, options)!;
				}
				else
				{
					value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
				}

				// Add to dictionary.
				dictionary.Add(key, value);
			}

			throw new JsonException();
		}

		public override void Write(
			Utf8JsonWriter writer,
			VersionedList<TValue> dictionary,
			JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			foreach ((UnityVersion key, TValue? value) in dictionary)
			{
				var propertyName = key.ToString();
				writer.WritePropertyName
					(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

				if (_valueConverter != null)
				{
					_valueConverter.Write(writer, value, options);
				}
				else
				{
					JsonSerializer.Serialize(writer, value, options);
				}
			}

			writer.WriteEndObject();
		}
	}
}
