using AssetRipper.Layout;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Meta;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.File;
using AssetRipper.Parser.Files.File.Version;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Structure.ProjectCollection.Collections;

namespace AssetRipper.Converters.Project
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
