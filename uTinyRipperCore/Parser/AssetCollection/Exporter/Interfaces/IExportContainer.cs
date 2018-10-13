using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(Object asset);
		AssetType ToExportType(ClassIDType classID);
		ExportPointer CreateExportPointer(Object asset);

		string SceneIndexToName(int sceneID);
		string TagIDToName(int tagID);

		IExportCollection CurrentCollection { get; }
		Version Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
