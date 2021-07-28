using AssetRipper.Layout;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Meta;
using AssetRipper.Classes.Object;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.Structure.Collections;

namespace AssetRipper.Project
{
#warning TODO: remove
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(UnityObject asset);
		AssetType ToExportType(ClassIDType classID);
		MetaPtr CreateExportPointer(UnityObject asset);

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
