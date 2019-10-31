using uTinyRipper.Project;
using uTinyRipper.Classes;

namespace uTinyRipper.Converters
{
#warning TODO: remove
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(Object asset);
		AssetType ToExportType(ClassIDType classID);
		MetaPtr CreateExportPointer(Object asset);

		string SceneIndexToName(int sceneID);
		bool IsSceneDuplicate(int sceneID);
		string TagIDToName(int tagID);

		IExportCollection CurrentCollection { get; }
		Version ExportVersion { get; }
		Platform ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
