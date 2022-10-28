using AssetRipper.Assets;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_91;

namespace AssetRipper.Library.Exporters.AnimatorControllers
{
	public sealed class AnimatorControllerExportCollection : AssetsExportCollection
	{
		public AnimatorControllerExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset.MainAsset!)
		{
			IAnimatorController controller = (IAnimatorController?)asset.MainAsset ?? throw new NullReferenceException();
			foreach (IUnityObjectBase? dependency in controller.FetchEditorHierarchy())
			{
				if (dependency is not null && dependency != controller)
				{
					AddAsset(dependency);
				}
			}
		}
	}
}
