using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.AnimatorControllers;

public sealed class AnimatorControllerExportCollection : AssetsExportCollection<IAnimatorController>
{
	public AnimatorControllerExportCollection(IAssetExporter assetExporter, IAnimatorController controller) : base(assetExporter, controller)
	{
		AddAssets(controller.FetchEditorHierarchy());
	}
}
