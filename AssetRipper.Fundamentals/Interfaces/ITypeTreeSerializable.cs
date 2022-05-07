using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using System.Collections.Generic;

namespace AssetRipper.Core.Interfaces
{
	public interface ITypeTreeSerializable
	{
		List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex);
		List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex);
	}
}
