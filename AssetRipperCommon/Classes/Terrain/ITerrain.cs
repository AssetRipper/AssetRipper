using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.TerrainData;

namespace AssetRipper.Core.Classes.Terrain
{
	public interface ITerrain : IBehaviour
	{
		PPtr<ITerrainData> TerrainData { get; }
	}
}
