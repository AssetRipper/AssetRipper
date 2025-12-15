using AssetRipper.Primitives;

namespace AssetRipper.SerializationLogic.Tests;

public static class SerializableTypes
{
	public static SerializableType Create<T>() => Create<T>(DefaultUnityVersion);
	public static SerializableType Create<T>(UnityVersion version) => Create(typeof(T), version);
	public static SerializableType Create(Type type) => Create(type, DefaultUnityVersion);
	public static SerializableType Create(Type type, UnityVersion version) => Create(ReferenceAssemblies.GetType(type), version);
	public static SerializableType Create(TypeDefinition type) => Create(type, DefaultUnityVersion);
	public static SerializableType Create(TypeDefinition type, UnityVersion version)
	{
		FieldSerializer serializer = new(version);
		if (serializer.TryCreateSerializableType(type, out SerializableType? serializableType, out string? failureReason))
		{
			return serializableType;
		}
		else
		{
			Assert.Fail($"Failed to create serializable type: {failureReason}");
			return default!;
		}
	}

	public static List<SerializableType> CreateMultiple<T>() => CreateMultiple<T>(DefaultUnityVersion);
	public static List<SerializableType> CreateMultiple<T>(UnityVersion version) => CreateMultiple(typeof(T), version);
	public static List<SerializableType> CreateMultiple(Type type) => CreateMultiple(type, DefaultUnityVersion);
	public static List<SerializableType> CreateMultiple(Type type, UnityVersion version) => CreateMultiple(ReferenceAssemblies.GetType(type), version);
	public static List<SerializableType> CreateMultiple(TypeDefinition type) => CreateMultiple(type, DefaultUnityVersion);
	public static List<SerializableType> CreateMultiple(TypeDefinition type, UnityVersion version)
	{
		FieldSerializer serializer = new(version);
		Dictionary<ITypeDefOrRef, SerializableType> typeCache = [];
		if (serializer.TryCreateSerializableType(type, typeCache, out _, out string? failureReason))
		{
			return typeCache.Values.ToList();
		}
		else
		{
			Assert.Fail($"Failed to create serializable type: {failureReason}");
			return [];
		}
	}

	/// <summary>
	/// Assume a recent Unity version if not specified.
	/// </summary>
	private static UnityVersion DefaultUnityVersion => new(6000);
}
