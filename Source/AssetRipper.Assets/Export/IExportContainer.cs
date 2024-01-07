using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Export
{
	public interface IExportContainer
	{
		long GetExportID(IUnityObjectBase asset);
		AssetType ToExportType(Type type);
		MetaPtr CreateExportPointer(IUnityObjectBase asset);

		UnityGuid ScenePathToGUID(string name);
		bool IsSceneDuplicate(int sceneID);

		AssetCollection File { get; }

		UnityVersion ExportVersion { get; }
		BuildTarget ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
