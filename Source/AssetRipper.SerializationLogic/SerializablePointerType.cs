namespace AssetRipper.SerializationLogic;

public sealed class SerializablePointerType : SerializableType
{
	public static SerializablePointerType Shared { get; } = new();

	private SerializablePointerType() : base("UnityEngine", PrimitiveType.Complex, "Object")
	{
		MaxDepth = 0;
	}
}
