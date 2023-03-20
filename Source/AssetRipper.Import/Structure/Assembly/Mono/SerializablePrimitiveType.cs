using AssetRipper.Import.Structure.Assembly.Serializable;

namespace AssetRipper.Import.Structure.Assembly.Mono;

internal sealed class SerializablePrimitiveType : SerializableType
{
	private static readonly Dictionary<PrimitiveType, SerializablePrimitiveType> cache = new();

	private SerializablePrimitiveType(PrimitiveType primitiveType) : base("System", primitiveType, primitiveType.ToSystemTypeName())
	{
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
