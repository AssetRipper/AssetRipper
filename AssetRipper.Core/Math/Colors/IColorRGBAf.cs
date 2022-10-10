using AssetRipper.Assets.IO;

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
