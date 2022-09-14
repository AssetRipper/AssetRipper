using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public sealed class TypeTree_19 : TypeTree<TypeTreeNode_19>
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
