using System.Collections.Concurrent;

namespace AssetRipper.SerializationLogic;

public sealed class SerializablePrimitiveType : SerializableType
{
	private static readonly ConcurrentDictionary<PrimitiveType, SerializablePrimitiveType> cache = new();

	private SerializablePrimitiveType(PrimitiveType primitiveType) : base("System", primitiveType, primitiveType.ToSystemTypeName())
	{
		MaxDepth = 0;
	}

	public static SerializablePrimitiveType GetOrCreate(PrimitiveType primitiveType)
	{
		return cache.GetOrAdd(primitiveType, static key => new SerializablePrimitiveType(key));
	}
}
