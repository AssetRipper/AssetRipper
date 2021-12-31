using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.OcclusionCullingData
{
	public interface ISceneObjectIdentifier : IUnityAssetBase
	{
		long TargetObject { get; set; }
		long TargetPrefab { get; set; }
	}
}
