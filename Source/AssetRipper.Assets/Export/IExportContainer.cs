using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.VersionUtilities;

namespace AssetRipper.Assets.Export
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(IUnityObjectBase asset);
		AssetType ToExportType(Type type);
		MetaPtr CreateExportPointer(IUnityObjectBase asset);

		UnityGUID SceneNameToGUID(string name);
		string SceneIndexToName(int sceneID);
		bool IsSceneDuplicate(int sceneID);
		string TagIDToName(int tagID);
		ushort TagNameToID(string tagName);

		AssetCollection File { get; }
		TemporaryAssetCollection VirtualFile { get; }

		IExportCollection CurrentCollection { get; }
		UnityVersion ExportVersion { get; }
		BuildTarget ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
