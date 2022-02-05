using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector2f : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
	}
}
