using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Tests;

internal class TypeTreeNodeStructTests
{
	[Test]
	public async Task NamedVectorTest()
	{
		TypeTree tree = new();
		tree.Nodes.Add(new TypeTreeNode("MonoBehaviour", "Base", 0, true));
		tree.Nodes.Add(new TypeTreeNode("SerializableClass", "fieldName", 1, true));
		tree.Nodes.Add(new TypeTreeNode("Array", "Array", 2, false));
		tree.Nodes.Add(new TypeTreeNode("int", "size", 3, false));
		tree.Nodes.Add(new TypeTreeNode("SerializableClass", "data", 3, true));
		tree.Nodes.Add(new TypeTreeNode("float", "subFieldName1", 4, false));
		tree.Nodes.Add(new TypeTreeNode("bool", "subFieldName2", 4, true));

		await Assert.That(TypeTreeNodeStruct.TryMakeFromTypeTree(tree, out TypeTreeNodeStruct rootNode)).IsTrue();
		await Assert.That(rootNode.SubNodes).Count().IsEqualTo(1);
		TypeTreeNodeStruct mainNode = rootNode.SubNodes[0];
		await Assert.That(mainNode.IsNamedVector).IsTrue();
	}

	[Test]
	[Arguments(PrimitiveType.Bool)]
	[Arguments(PrimitiveType.Char)]
	[Arguments(PrimitiveType.SByte)]
	[Arguments(PrimitiveType.Byte)]
	[Arguments(PrimitiveType.Short)]
	[Arguments(PrimitiveType.UShort)]
	[Arguments(PrimitiveType.Int)]
	[Arguments(PrimitiveType.UInt)]
	[Arguments(PrimitiveType.Long)]
	[Arguments(PrimitiveType.ULong)]
	[Arguments(PrimitiveType.Single)]
	[Arguments(PrimitiveType.Double)]
	[Arguments(PrimitiveType.String)]
	public async Task PrimitiveTypeRoundTripIsTheSame(PrimitiveType primitiveType)
	{
		SerializablePrimitiveType serializableType = SerializablePrimitiveType.GetOrCreate(primitiveType);
		TypeTreeNodeStruct node = TypeTreeNodeStruct.FromSerializableType(serializableType);
		SerializableTreeType serializableTreeType = SerializableTreeType.FromRootNode(node);
		await Assert.That(serializableTreeType.Type).IsEqualTo(primitiveType);
	}
}