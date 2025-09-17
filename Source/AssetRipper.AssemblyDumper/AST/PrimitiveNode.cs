namespace AssetRipper.AssemblyDumper.AST;

internal sealed class PrimitiveNode : Node
{
	public PrimitiveNode(TypeSignature typeSignature, Node parent) : base(parent)
	{
		TypeSignature = typeSignature;
	}

	public override TypeSignature TypeSignature { get; }

	public override bool AnyPPtrs => false;

	/// <summary>
	/// Byte arrays do not implement <see cref="IEquatable{T}"/>, but other primitive types do.
	/// </summary>
	public override bool Equatable => TypeSignature is not SzArrayTypeSignature;
}
