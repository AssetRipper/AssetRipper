using AssetRipper.Core.Structure.Assembly.Serializable;

namespace AssetRipper.Core.Structure.Assembly.Mono;

internal sealed class SerializablePrimitiveType : SerializableType
{
	public SerializablePrimitiveType(PrimitiveType primitiveType) : base("System", primitiveType, primitiveType.ToSystemTypeName())
	{
	}
}
