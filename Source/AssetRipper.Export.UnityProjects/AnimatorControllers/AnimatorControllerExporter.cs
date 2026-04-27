using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.AnimatorControllers;

public sealed class AnimatorControllerExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		switch (asset.MainAsset)
		{
			case IAnimatorController controller:
				exportCollection = new AnimatorControllerExportCollection(this, controller);
				return true;
			case IAnimatorOverrideController overrideController:
				exportCollection = new AnimatorOverrideControllerExportCollection(this, overrideController);
				return true;
			default:
				exportCollection = null;
				return false;
		}
	}

	private sealed class AnimatorControllerExportCollection : AssetsExportCollection<IAnimatorController>
	{
		public AnimatorControllerExportCollection(IAssetExporter assetExporter, IAnimatorController controller) : base(assetExporter, controller)
		{
			AddAssets(controller.FetchEditorHierarchy());
		}
	}

	private sealed class AnimatorOverrideControllerExportCollection : AssetsExportCollection<IAnimatorOverrideController>
	{
		public AnimatorOverrideControllerExportCollection(IAssetExporter assetExporter, IAnimatorOverrideController controller) : base(assetExporter, controller)
		{
			AddAssets(controller.FetchEditorHierarchy());
		}
	}
}
