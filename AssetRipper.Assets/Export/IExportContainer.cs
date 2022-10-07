using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.VersionUtilities;

namespace AssetRipper.Assets.Export
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(IUnityObjectBase asset);
		AssetType ToExportType(Type type);
		MetaPtr CreateExportPointer(IUnityObjectBase asset);

		string SceneIndexToName(int sceneID);
		bool IsSceneDuplicate(int sceneID);
		string TagIDToName(int tagID);
		ushort TagNameToID(string tagName);

		IExportCollection CurrentCollection { get; }
		UnityVersion ExportVersion { get; }
		BuildTarget ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
