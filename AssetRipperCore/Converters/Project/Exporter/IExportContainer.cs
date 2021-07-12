using AssetRipper.Project;
using AssetRipper.Classes;
using AssetRipper.Layout;

namespace AssetRipper.Converters
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
		ushort TagNameToID(string tagName);

		IExportCollection CurrentCollection { get; }
		AssetLayout ExportLayout { get; }
		Version ExportVersion { get; }
		Platform ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
