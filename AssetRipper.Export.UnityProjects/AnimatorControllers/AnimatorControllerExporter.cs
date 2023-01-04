using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_91;

namespace AssetRipper.Export.UnityProjects.AnimatorControllers
{
	public sealed class AnimatorControllerExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset.MainAsset is IAnimatorController;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new AnimatorControllerExportCollection(this, asset);
		}
	}
}
