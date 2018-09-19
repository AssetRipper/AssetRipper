using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.AssetExporters
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(Object asset);
		AssetType ToExportType(ClassIDType classID);
		ExportPointer CreateExportPointer(Object @object);

		string SceneIndexToName(int sceneID);
		string TagIDToName(int tagID);

		IExportCollection CurrentCollection { get; }
		Version Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }
	}
}
