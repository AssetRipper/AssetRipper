using AssetRipper.Core.Math;

namespace AssetRipper.Core.Classes.TerrainData
{
	public interface IHeightmap
	{
		int Width { get; }
		int Height { get; }
		short[] Heights { get; set; }
		Vector3f Scale { get; }
	}
}
