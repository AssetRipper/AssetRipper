using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector3i : IAsset
	{
		int X { get; set; }
		int Y { get; set; }
		int Z { get; set; }
	}
}
