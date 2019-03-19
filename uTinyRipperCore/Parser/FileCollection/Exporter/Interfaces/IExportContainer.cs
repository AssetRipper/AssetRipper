using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
#warning TODO: remove
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(Object asset);
		AssetType ToExportType(ClassIDType classID);
		ExportPointer CreateExportPointer(Object asset);

		string SceneIndexToName(int sceneID);
		bool IsSceneDuplicate(int sceneID);
		string TagIDToName(int tagID);

		IExportCollection CurrentCollection { get; }
		Version Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }
		Version ExportVersion { get; }
		Platform ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
