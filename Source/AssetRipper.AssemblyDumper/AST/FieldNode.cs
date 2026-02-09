using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.AST;

internal sealed class FieldNode : SingleNode<Node>
{
	public FieldNode(ClassProperty property, Node? parent = null) : base(parent)
	{
		Debug.Assert(property.BackingField is not null);
		Property = property;
		Child = Create(TypeSignature, this);
	}

	public ClassProperty Property { get; }

	public FieldDefinition Field => Property.BackingField!;
	public override TypeSignature TypeSignature => Field.Signature!.FieldType;
}
