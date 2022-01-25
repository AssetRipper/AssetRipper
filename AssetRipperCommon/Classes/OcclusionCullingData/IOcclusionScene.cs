using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.OcclusionCullingData
{
	public interface IOcclusionScene : IUnityAssetBase
	{
		public int IndexRenderers { get; set; }
		public int SizeRenderers { get; set; }
		public int IndexPortals { get; set; }
		public int SizePortals { get; set; }
		public UnityGUID Scene { get; set; }
	}

	public static class OcclusionSceneExtensions
	{
		public static void Initialize(this IOcclusionScene occlusionScene, UnityGUID scene, int renderSize, int portalSize)
		{
			occlusionScene.Scene = scene;
			occlusionScene.IndexRenderers = 0;
			occlusionScene.SizeRenderers = renderSize;
			occlusionScene.IndexPortals = 0;
			occlusionScene.SizePortals = portalSize;
		}
	}
}
