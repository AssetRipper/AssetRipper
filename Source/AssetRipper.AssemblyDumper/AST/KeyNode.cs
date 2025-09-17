namespace AssetRipper.AssemblyDumper.AST;

internal sealed class KeyNode : SingleNode<Node>
{
	public KeyNode(TypeSignature typeSignature, Node? parent = null) : base(parent)
	{
		Child = Create(typeSignature, this);
	}

	public override TypeSignature TypeSignature => Child.TypeSignature;
}
