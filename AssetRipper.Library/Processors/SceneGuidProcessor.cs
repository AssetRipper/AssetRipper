using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Core.Linq;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_29;

namespace AssetRipper.Library.Processors
{
	public sealed class SceneGuidProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Scene GUID Assignment");
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				foreach (IOcclusionCullingSettings sceneSettings in collection.SelectType<IUnityObjectBase, IOcclusionCullingSettings>())
				{
					if (sceneSettings.Has_SceneGUID_C29())
					{
						collection.GUID = sceneSettings.SceneGUID_C29;
					}
				}
				if (collection.GUID.IsZero)
				{
					collection.GUID = UnityGUID.NewGuid();
				}
			}
		}
	}
}
