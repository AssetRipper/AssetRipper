namespace AssetRipper.SerializationLogic;

/// <summary>
/// Type tree representation of a <c>[SerializeReference]</c> field: a single <c>SInt64 rid</c>.
/// </summary>
public sealed class ManagedReferenceType : SerializableType
{
	public static ManagedReferenceType Shared { get; } = new();

	private ManagedReferenceType() : base(null, PrimitiveType.Complex, "managedReference")
	{
		Fields = [new Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long), 0, "rid", false)];
		MaxDepth = 1;
	}
}

/// <summary>
/// Marker type for the trailing <c>ManagedReferencesRegistry</c> on assets that use <c>[SerializeReference]</c>.
/// Actual binary layout is handled specially during read/write.
/// </summary>
public sealed class ManagedReferencesRegistryType : SerializableType
{
	public static ManagedReferencesRegistryType Shared { get; } = new();

	private ManagedReferencesRegistryType() : base(null, PrimitiveType.Complex, "ManagedReferencesRegistry")
	{
		Fields = [];
		MaxDepth = 0;
	}
}
