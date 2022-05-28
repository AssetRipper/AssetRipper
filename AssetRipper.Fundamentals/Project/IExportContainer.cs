using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project
{
	public interface IExportContainer : IAssetContainer
	{
		long GetExportID(IUnityObjectBase asset);
		AssetType ToExportType(Type type);
		MetaPtr CreateExportPointer(IUnityObjectBase asset);

		string SceneIndexToName(int sceneID);
		bool IsSceneDuplicate(int sceneID);
		string TagIDToName(int tagID);
		ushort TagNameToID(string tagName);

		IExportCollection CurrentCollection { get; }
		LayoutInfo ExportLayout { get; }
		UnityVersion ExportVersion { get; }
		BuildTarget ExportPlatform { get; }
		TransferInstructionFlags ExportFlags { get; }
	}
}
