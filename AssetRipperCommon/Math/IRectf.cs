using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math
{
	public interface IRectf : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
		float Width { get; set; }
		float Height { get; set; }
	}
}
