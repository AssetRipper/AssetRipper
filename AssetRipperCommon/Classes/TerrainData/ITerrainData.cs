using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.TerrainData
{
	public interface ITerrainData : IUnityObjectBase, IHasName
	{
		IHeightmap Heightmap { get; }
	}
}
