using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AssetRipper.GUI.Web.Documentation;

/// <summary>
/// A <see cref="JsonSerializerContext"/> that treats everything as { }.
/// </summary>
/// <remarks>
/// Note: Classes have to be individually declared.
/// </remarks>
internal sealed class NullSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	public static NullSerializerContext Instance { get; } = new(new(new JsonSerializerOptions()));

	public NullSerializerContext(JsonSerializerOptions? options) : base(options)
	{
	}

	protected override JsonSerializerOptions? GeneratedSerializerOptions => null;

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		Options.TryGetTypeInfo(type, out JsonTypeInfo? typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(Task))
		{
			return CreateTypeInfo(options, () => Task.CompletedTask);
		}
		return null;
	}

	private static JsonTypeInfo<T> CreateTypeInfo<T>(JsonSerializerOptions options, Func<T> objectCreator) where T : class
	{
		JsonObjectInfoValues<T> jsonObjectInfoValues = new()
		{
			ObjectCreator = objectCreator,
			ObjectWithParameterizedConstructorCreator = null,
			PropertyMetadataInitializer = (_) => [],
			ConstructorParameterMetadataInitializer = null,
			ConstructorAttributeProviderFactory = null,
			SerializeHandler = null,
		};
		JsonTypeInfo<T> jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, jsonObjectInfoValues);
		jsonTypeInfo.NumberHandling = null;
		return jsonTypeInfo;
	}
}
