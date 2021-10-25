using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(Object asset);
		AssetType ToExportType(ClassIDType classID);
		MetaPtr CreateExportPointer(UnityObjectBase asset);

		string SceneIndexToName(int sceneID);
		bool IsSceneDuplicate(int sceneID);
		string TagIDToName(int tagID);
		ushort TagNameToID(string tagName);

		IExportCollection CurrentCollection { get; }
		AssetLayout ExportLayout { get; }
		UnityVersion ExportVersion { get; }
		Platform ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
