namespace AssetRipper.AssemblyDumper.AST;

internal abstract class SingleNode<TChild> : Node where TChild : Node
{
	public SingleNode(Node? parent = null) : base(parent)
	{
	}

	public TChild Child { get; set; } = null!;

	public sealed override IReadOnlyList<Node> Children => [Child];

	public sealed override bool AnyPPtrs => Child.AnyPPtrs;

	public override bool Equatable => Child.Equatable;
}
