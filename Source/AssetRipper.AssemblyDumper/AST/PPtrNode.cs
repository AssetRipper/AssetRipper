namespace AssetRipper.AssemblyDumper.AST;

internal sealed class PPtrNode : Node
{
	public PPtrNode(GeneratedClassInstance pptrType, Node parent) : base(parent)
	{
		ClassInstance = pptrType;
	}

	public GeneratedClassInstance ClassInstance { get; }

	public override TypeSignature TypeSignature => ClassInstance.Type.ToTypeSignature();

	public override bool AnyPPtrs => true;

	public override bool Equatable => false;
}
