using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.SerializedFiles;

namespace AssetRipper.Core.Project
{
	public interface IProjectAssetContainer : IExportContainer
	{
		ISerializedFile File { get; }
		VirtualSerializedFile VirtualFile { get; }
		UnityGUID SceneNameToGUID(string name);
	}
}
