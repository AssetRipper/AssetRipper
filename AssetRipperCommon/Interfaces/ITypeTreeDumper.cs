using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;

namespace AssetRipper.Core.Interfaces
{
	public interface ITypeTreeDumper
	{
		TypeTree MakeReleaseTypeTree();
		TypeTree MakeEditorTypeTree();
	}
}
