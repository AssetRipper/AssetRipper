namespace AssetRipper.AssemblyDumper.AST;

internal sealed class TypeNode : Node
{
	public TypeNode(GeneratedClassInstance classInstance, Node? parent = null) : base(parent)
	{
		ClassInstance = classInstance;
		List<FieldNode> children = new();
		foreach (ClassProperty property in classInstance.Properties.Where(p => p.BackingField is not null))
		{
			children.Add(new FieldNode(property, this));
		}
		Children = children.Count > 0 ? children : [];
	}

	public GeneratedClassInstance ClassInstance { get; }

	public override IReadOnlyList<FieldNode> Children { get; }

	public override TypeSignature TypeSignature => ClassInstance.Type.ToTypeSignature();

	/// <summary>
	/// We generate the equality comparison for the type if it has no PPtrs.
	/// </summary>
	public override bool Equatable => !AnyPPtrs;
}
