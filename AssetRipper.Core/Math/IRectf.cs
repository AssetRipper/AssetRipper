using AssetRipper.Assets.IO;

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
