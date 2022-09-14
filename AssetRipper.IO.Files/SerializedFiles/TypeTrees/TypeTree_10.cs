using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public sealed class TypeTree_10 : TypeTree<TypeTreeNode_10>
{
	public override void Read(EndianReader reader)
	{
		ReadVersion10Plus(reader);
	}

	public override void Write(EndianWriter writer)
	{
		WriteVersion10Plus(writer);
	}
}
