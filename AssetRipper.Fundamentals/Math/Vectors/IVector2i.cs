using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector2i : IAsset
	{
		int X { get; set; }
		int Y { get; set; }
	}
}
