using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.OcclusionCullingSettings
{
	public interface ISceneSettings : IUnityObjectBase
	{
		UnityGUID SceneGUID { get; }
	}
}