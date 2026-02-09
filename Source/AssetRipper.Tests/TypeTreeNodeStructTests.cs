using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Tests;

internal class TypeTreeNodeStructTests
{
	[Test]
	public void NamedVectorTest()
	{
		TypeTree tree = new();
		tree.Nodes.Add(new TypeTreeNode("MonoBehaviour", "Base", 0, true));
		tree.Nodes.Add(new TypeTreeNode("SerializableClass", "fieldName", 1, true));
		tree.Nodes.Add(new TypeTreeNode("Array", "Array", 2, false));
		tree.Nodes.Add(new TypeTreeNode("int", "size", 3, false));
		tree.Nodes.Add(new TypeTreeNode("SerializableClass", "data", 3, true));
		tree.Nodes.Add(new TypeTreeNode("float", "subFieldName1", 4, false));
		tree.Nodes.Add(new TypeTreeNode("bool", "subFieldName2", 4, true));

		Assert.That(TypeTreeNodeStruct.TryMakeFromTypeTree(tree, out TypeTreeNodeStruct rootNode));
		Assert.That(rootNode.SubNodes, Has.Count.EqualTo(1));
		TypeTreeNodeStruct mainNode = rootNode.SubNodes[0];
		Assert.That(mainNode.IsNamedVector);
	}

	[TestCase(PrimitiveType.Bool)]
	[TestCase(PrimitiveType.Char)]
	[TestCase(PrimitiveType.SByte)]
	[TestCase(PrimitiveType.Byte)]
	[TestCase(PrimitiveType.Short)]
	[TestCase(PrimitiveType.UShort)]
	[TestCase(PrimitiveType.Int)]
	[TestCase(PrimitiveType.UInt)]
	[TestCase(PrimitiveType.Long)]
	[TestCase(PrimitiveType.ULong)]
	[TestCase(PrimitiveType.Single)]
	[TestCase(PrimitiveType.Double)]
	[TestCase(PrimitiveType.String)]
	public void PrimitiveTypeRoundTripIsTheSame(PrimitiveType primitiveType)
	{
		SerializablePrimitiveType serializableType = SerializablePrimitiveType.GetOrCreate(primitiveType);
		TypeTreeNodeStruct node = TypeTreeNodeStruct.FromSerializableType(serializableType);
		SerializableTreeType serializableTreeType = SerializableTreeType.FromRootNode(node);
		Assert.That(serializableTreeType.Type, Is.EqualTo(primitiveType));
	}
}
