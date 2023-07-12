using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Export
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(IUnityObjectBase asset);
		AssetType ToExportType(Type type);
		MetaPtr CreateExportPointer(IUnityObjectBase asset);

		UnityGuid SceneNameToGUID(string name);
		bool IsSceneDuplicate(int sceneID);

		AssetCollection File { get; }
		TemporaryAssetCollection VirtualFile { get; }

		IExportCollection CurrentCollection { get; }
		UnityVersion ExportVersion { get; }
		BuildTarget ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
