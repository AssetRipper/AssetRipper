using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;

namespace AssetRipper.Assets.Interfaces
{
	public interface ITypeTreeSerializable
	{
		List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex);
		List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex);
	}
}
