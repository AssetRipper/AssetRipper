using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Colors
{
	public interface IColorRGBA32 : IAsset
	{
		uint RGBA { get; set; }
	}
}
