using AssetRipper.Import.Structure.Assembly.Serializable;

namespace AssetRipper.Import.Structure.Assembly.Mono;

internal sealed class SerializablePrimitiveType : SerializableType
{
	public SerializablePrimitiveType(PrimitiveType primitiveType) : base("System", primitiveType, primitiveType.ToSystemTypeName())
	{
	}
}
