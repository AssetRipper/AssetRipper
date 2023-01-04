using AssetRipper.Import.Structure.Assembly.Serializable;

namespace AssetRipper.Import.Structure.Assembly.Mono;

internal sealed class SerializablePointerType : SerializableType
{
	public SerializablePointerType() : base("UnityEngine", PrimitiveType.Complex, "Object")
	{
	}
}
