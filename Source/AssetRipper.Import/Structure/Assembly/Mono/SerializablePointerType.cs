using AssetRipper.Import.Structure.Assembly.Serializable;

namespace AssetRipper.Import.Structure.Assembly.Mono;

public sealed class SerializablePointerType : SerializableType
{
	public static SerializablePointerType Shared { get; } = new();

	private SerializablePointerType() : base("UnityEngine", PrimitiveType.Complex, "Object")
	{
	}
}
