using AssetRipper.Core.Classes.OcclusionCullingData;

namespace AssetRipper.Core.Parser.Asset
{
	public class SceneObjectFactory : SceneObjectFactoryBase
	{
		public override IOcclusionScene CreateOcclusionScene()
		{
			return new OcclusionScene();
		}

		public override ISceneObjectIdentifier CreateSceneObjectIdentifier()
		{
			return new SceneObjectIdentifier();
		}
	}
}
