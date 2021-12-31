using AssetRipper.Core.Classes.OcclusionCullingData;

namespace AssetRipper.Core.Parser.Asset
{
	public abstract class SceneObjectFactoryBase
	{
		public abstract ISceneObjectIdentifier CreateSceneObjectIdentifier();
		public ISceneObjectIdentifier CreateSceneObjectIdentifier(long targetObject, long targetPrefab)
		{
			ISceneObjectIdentifier sceneObjectIdentifier = CreateSceneObjectIdentifier();
			sceneObjectIdentifier.TargetObject = targetObject;
			sceneObjectIdentifier.TargetPrefab = targetPrefab;
			return sceneObjectIdentifier;
		}

		public abstract IOcclusionScene CreateOcclusionScene();
	}
}
