namespace AssetRipper.SerializationLogic;

/// <summary>A reference ID into the owning Unity object's managed reference registry.</summary>
public sealed class SerializableManagedReferenceType : SerializableType
{
	public static SerializableManagedReferenceType Instance { get; } = new();

	private SerializableManagedReferenceType() : base(null, PrimitiveType.Complex, "managedReference")
	{
		Fields = [new(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long), 0, "rid", false)];
		MaxDepth = 1;
	}

	public override bool FlowMappedInYaml => true;
}
