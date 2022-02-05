using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Colors
{
	public interface IColorRGBAf : IAsset
	{
		float R { get; set; }
		float G { get; set; }
		float B { get; set; }
		float A { get; set; }
	}
}
