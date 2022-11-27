using AssetRipper.Core.Structure.Assembly.Serializable;

namespace AssetRipper.Core.Structure.Assembly.Mono;

internal sealed class SerializablePointerType : SerializableType
{
	public SerializablePointerType() : base("UnityEngine", PrimitiveType.Complex, "Object")
	{
	}
}
