using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public partial interface ITypeTreeNode
{
	void Read(EndianReader reader);
}
