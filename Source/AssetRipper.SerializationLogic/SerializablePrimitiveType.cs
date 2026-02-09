namespace AssetRipper.SerializationLogic;

public sealed class SerializablePrimitiveType : SerializableType
{
	private static readonly Dictionary<PrimitiveType, SerializablePrimitiveType> cache = new();

	private SerializablePrimitiveType(PrimitiveType primitiveType) : base("System", primitiveType, primitiveType.ToSystemTypeName())
	{
		MaxDepth = 0;
	}

	public static SerializablePrimitiveType GetOrCreate(PrimitiveType primitiveType)
	{
		if (!cache.TryGetValue(primitiveType, out SerializablePrimitiveType? result))
		{
			result = new SerializablePrimitiveType(primitiveType);
			cache.Add(primitiveType, result);
		}
		return result;
	}
}
